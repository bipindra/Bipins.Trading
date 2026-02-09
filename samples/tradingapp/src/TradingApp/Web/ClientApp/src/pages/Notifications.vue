<template>
  <div class="page">
    <h1 class="page-title">Notifications</h1>
    <div class="toolbar">
      <label class="toggle">
        <input v-model="unreadOnly" type="checkbox" />
        Unread only
      </label>
      <button v-if="desktopPermission === 'default'" type="button" class="btn-small" @click="requestDesktopNotifications">
        Enable desktop alerts
      </button>
    </div>
    <div v-if="loading && items.length === 0" class="loading">Loading...</div>
    <div v-else-if="items.length === 0" class="empty">
      <p>{{ unreadOnly ? 'No unread notifications.' : 'No notifications yet.' }}</p>
    </div>
    <ul v-else class="list">
      <li v-for="n in items" :key="n.id" class="item" :class="{ unread: !n.readAt }">
        <div class="row">
          <span class="symbol">{{ n.symbol }}</span>
          <span class="time">{{ formatTime(n.triggeredAt) }}</span>
        </div>
        <p class="message">{{ n.message }}</p>
        <button v-if="!n.readAt" type="button" class="btn-mark" @click="markRead(n)">Mark read</button>
      </li>
    </ul>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted, watch } from 'vue'
import { api } from '../api/client'
import { useToastStore } from '../stores/toast'

const toast = useToastStore()
const items = ref([])
const loading = ref(true)
const unreadOnly = ref(false)
const desktopPermission = ref(typeof Notification !== 'undefined' ? Notification.permission : 'denied')
let pollTimer = null
let lastSeenIds = new Set()

async function fetchNotifications() {
  try {
    const res = await api.notifications.get(50, unreadOnly.value)
    const list = res?.items ?? []
    if (!unreadOnly.value && list.length > 0 && desktopPermission.value === 'granted') {
      const newOnes = list.filter((n) => !lastSeenIds.has(n.id))
      newOnes.forEach((n) => {
        try {
          new Notification(n.symbol, { body: n.message })
        } catch (_) {}
      })
    }
    items.value = list
    list.forEach((n) => lastSeenIds.add(n.id))
  } catch (e) {
    if (items.value.length === 0) toast.error(e.message || 'Failed to load notifications')
  } finally {
    loading.value = false
  }
}

function formatTime(iso) {
  if (!iso) return ''
  const d = new Date(iso)
  const now = new Date()
  const sameDay = d.toDateString() === now.toDateString()
  return sameDay ? d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }) : d.toLocaleDateString()
}

async function markRead(n) {
  try {
    await api.notifications.markRead(n.id)
    n.readAt = new Date().toISOString()
    toast.success('Marked read')
  } catch (e) {
    toast.error(e.message || 'Failed to mark read')
  }
}

function requestDesktopNotifications() {
  if (typeof Notification === 'undefined') return
  Notification.requestPermission().then((p) => {
    desktopPermission.value = p
    if (p === 'granted') toast.success('Desktop notifications enabled')
  })
}

function startPolling() {
  pollTimer = setInterval(fetchNotifications, 30000)
}

function stopPolling() {
  if (pollTimer) clearInterval(pollTimer)
  pollTimer = null
}

onMounted(() => {
  fetchNotifications()
  startPolling()
})

onUnmounted(stopPolling)

watch(unreadOnly, () => {
  loading.value = true
  fetchNotifications()
})
</script>

<style scoped>
.page-title {
  margin: 0 0 16px;
  font-size: 24px;
  font-weight: 600;
}

.toolbar {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 16px;
  flex-wrap: wrap;
}

.toggle {
  display: flex;
  align-items: center;
  gap: 8px;
  color: var(--text-muted);
  font-size: 14px;
  cursor: pointer;
}

.btn-small {
  padding: 6px 12px;
  font-size: 13px;
  background: var(--aqua-dim);
  color: var(--aqua);
  border: none;
  border-radius: 8px;
  cursor: pointer;
}

.btn-small:hover {
  background: rgba(0, 212, 170, 0.25);
}

.loading,
.empty {
  padding: 32px 0;
  text-align: center;
  color: var(--text-muted);
}

.list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.item {
  background: var(--bg-card);
  border-radius: var(--radius);
  padding: 12px;
  margin-bottom: 8px;
}

.item.unread {
  border-left: 3px solid var(--aqua);
}

.row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.symbol {
  font-weight: 600;
  color: var(--aqua);
}

.time {
  font-size: 12px;
  color: var(--text-muted);
}

.message {
  margin: 0 0 8px;
  font-size: 14px;
  color: var(--text-muted);
}

.btn-mark {
  padding: 4px 10px;
  font-size: 12px;
  background: transparent;
  color: var(--aqua);
  border: 1px solid var(--aqua);
  border-radius: 6px;
  cursor: pointer;
}

.btn-mark:hover {
  background: var(--aqua-dim);
}
</style>
