<template>
  <div class="page">
    <h1 class="page-title">Activity Monitor</h1>
    <div class="toolbar">
      <select v-model="selectedCategory" class="select">
        <option value="">All Categories</option>
        <option value="AlertWatch">Alert Watch</option>
        <option value="QuoteIngestion">Quote Ingestion</option>
        <option value="SignalGeneration">Signal Generation</option>
      </select>
      <select v-model="selectedLevel" class="select">
        <option value="">All Levels</option>
        <option value="Debug">Debug</option>
        <option value="Info">Info</option>
        <option value="Warning">Warning</option>
        <option value="Error">Error</option>
      </select>
      <button type="button" class="btn-small" @click="deleteAllLogs" :disabled="deletingAll">
        {{ deletingAll ? 'Clearing...' : 'Clear' }}
      </button>
    </div>
    <div v-if="loading && filteredLogs.length === 0" class="loading">Loading...</div>
    <div v-else-if="filteredLogs.length === 0" class="empty">
      <p>No activity logs yet.</p>
    </div>
    <div v-else class="logs-container">
      <div
        v-for="log in filteredLogs"
        :key="`${clearKey}-${log.id}-${log.timestamp}`"
        class="log-item"
        :class="[`level-${log.level.toLowerCase()}`, `category-${log.category.toLowerCase()}`]"
      >
        <div class="log-header">
          <span class="log-time">{{ formatTime(log.timestamp) }}</span>
          <span class="log-level" :class="`level-${log.level.toLowerCase()}`">{{ log.level }}</span>
          <span class="log-category">{{ log.category }}</span>
          <span v-if="log.symbol" class="log-symbol">{{ log.symbol }}</span>
          <span v-if="log.alertId" class="log-alert-id">Alert #{{ log.alertId }}</span>
        </div>
        <div class="log-message">{{ log.message }}</div>
        <div v-if="log.details" class="log-details">
          <details>
            <summary>Details</summary>
            <pre>{{ formatDetails(log.details) }}</pre>
          </details>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, computed, watch, nextTick } from 'vue'
import * as signalR from '@microsoft/signalr'
import { api } from '../api/client'
import { useToastStore } from '../stores/toast'

const toast = useToastStore()
const logs = ref([])
const loading = ref(true)
const selectedCategory = ref('')
const selectedLevel = ref('')
const clearKey = ref(0) // Force re-render when clearing
const deletingAll = ref(false) // Loading state for delete all operation
let logConnection = null
let activityLogHandler = null // Store handler reference so we can remove it

const filteredLogs = computed(() => {
  let result = logs.value
  
  if (selectedCategory.value) {
    result = result.filter(l => l.category === selectedCategory.value)
  }
  if (selectedLevel.value) {
    result = result.filter(l => l.level === selectedLevel.value)
  }
  return result
})

function formatTime(iso) {
  if (!iso) return ''
  const d = new Date(iso)
  return d.toLocaleString(undefined, {
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit'
  })
}

function formatDetails(details) {
  try {
    const parsed = JSON.parse(details)
    return JSON.stringify(parsed, null, 2)
  } catch {
    return details
  }
}

async function fetchLogs() {
  try {
    const res = await api.activityLogs.get(200, selectedCategory.value || null)
    logs.value = res?.logs ?? []
  } catch (e) {
    toast.error(e.message || 'Failed to load activity logs')
    logs.value = []
  } finally {
    loading.value = false
  }
}

async function deleteAllLogs() {
  if (!confirm('Are you sure you want to delete ALL activity logs from the database? This action cannot be undone.')) {
    return
  }
  
  deletingAll.value = true
  try {
    // Delete all logs from database
    await api.activityLogs.deleteAll()
    
    // Clear the UI immediately
    logs.value = []
    clearKey.value++
    
    // Reset filters
    selectedCategory.value = ''
    selectedLevel.value = ''
    
    // Reload logs from database (should be empty now)
    await fetchLogs()
    
    toast.success('All activity logs cleared')
  } catch (error) {
    console.error('Failed to delete all logs:', error)
    toast.error(`Failed to clear logs: ${error.message}`)
  } finally {
    deletingAll.value = false
  }
}

function startSignalRConnection() {
  const baseUrl = window.location.origin
  logConnection = new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/activity-log`)
    .withAutomaticReconnect()
    .build()
  
  // Create handler function
  activityLogHandler = (log) => {
    // Add new log to the beginning of the array
    logs.value.unshift(log)
    // Keep only the most recent 500 logs to prevent memory issues
    if (logs.value.length > 500) {
      logs.value = logs.value.slice(0, 500)
    }
  }
  
  logConnection.on('ActivityLog', activityLogHandler)
  
  logConnection.onreconnected(() => {
    console.log('Activity log SignalR connection reconnected')
    // Reload logs on reconnect
    fetchLogs()
  })
  
  logConnection.start()
    .then(() => {
      console.log('Activity log SignalR connection started')
    })
    .catch((err) => {
      console.warn('Activity log SignalR connection failed:', err)
      toast.error('Failed to connect to real-time activity logs')
    })
}

watch([selectedCategory, selectedLevel], () => {
  // Filtering is handled by computed property, no need to refetch
})

onMounted(() => {
  // Load initial logs from API
  fetchLogs()
  // Start SignalR connection for real-time updates
  startSignalRConnection()
})

onUnmounted(() => {
  if (logConnection) {
    logConnection.stop()
    logConnection = null
  }
})
</script>

<style scoped>
.page-title {
  margin: 0 0 20px;
  font-size: 24px;
  font-weight: 700;
}

.toolbar {
  display: flex;
  gap: 12px;
  align-items: center;
  flex-wrap: wrap;
  margin-bottom: 20px;
  padding-bottom: 16px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.select {
  padding: 8px 12px;
  background: var(--bg-card);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 6px;
  color: var(--text);
  font-size: 14px;
}

.btn-small {
  padding: 8px 16px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 6px;
  color: var(--text);
  font-size: 14px;
  cursor: pointer;
}

.btn-small:hover:not(:disabled) {
  background: rgba(255, 255, 255, 0.15);
}

.btn-small:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.loading,
.empty {
  padding: 40px 20px;
  text-align: center;
  color: var(--text-muted);
}

.logs-container {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-height: calc(100vh - 250px);
  overflow-y: auto;
}

.log-item {
  padding: 16px;
  background: var(--bg-card);
  border-radius: 8px;
  border-left: 4px solid rgba(255, 255, 255, 0.2);
  font-size: 13px;
}

.log-item.level-debug {
  border-left-color: rgba(255, 255, 255, 0.3);
  opacity: 0.8;
}

.log-item.level-info {
  border-left-color: var(--aqua);
}

.log-item.level-warning {
  border-left-color: #ffa500;
}

.log-item.level-error {
  border-left-color: #ff4747;
}

.log-header {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 8px;
  flex-wrap: wrap;
}

.log-time {
  color: var(--text-muted);
  font-size: 12px;
}

.log-level {
  padding: 2px 8px;
  border-radius: 4px;
  font-size: 11px;
  font-weight: 600;
  text-transform: uppercase;
}

.log-level.level-debug {
  background: rgba(255, 255, 255, 0.1);
  color: var(--text-muted);
}

.log-level.level-info {
  background: rgba(0, 217, 255, 0.2);
  color: var(--aqua);
}

.log-level.level-warning {
  background: rgba(255, 165, 0, 0.2);
  color: #ffa500;
}

.log-level.level-error {
  background: rgba(255, 71, 71, 0.2);
  color: #ff4747;
}

.log-category {
  padding: 2px 8px;
  background: rgba(255, 255, 255, 0.1);
  border-radius: 4px;
  font-size: 11px;
  color: var(--text-muted);
}

.log-symbol {
  font-weight: 600;
  color: var(--text);
}

.log-alert-id {
  color: var(--aqua);
  font-size: 11px;
}

.log-message {
  color: var(--text);
  line-height: 1.5;
  margin-bottom: 8px;
}

.log-details {
  margin-top: 8px;
  padding-top: 8px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
}

.log-details summary {
  cursor: pointer;
  color: var(--text-muted);
  font-size: 12px;
  margin-bottom: 4px;
}

.log-details pre {
  margin: 8px 0 0;
  padding: 8px;
  background: var(--bg-dark);
  border-radius: 4px;
  font-size: 11px;
  color: var(--text-muted);
  overflow-x: auto;
  white-space: pre-wrap;
  word-wrap: break-word;
}
</style>
