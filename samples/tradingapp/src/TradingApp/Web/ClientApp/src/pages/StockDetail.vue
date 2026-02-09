<template>
  <div class="page">
    <div v-if="loading" class="loading">Loading...</div>
    <template v-else-if="stock">
      <div class="header">
        <router-link to="/" class="back" aria-label="Back">
          <ArrowLeftIcon class="back-icon" />
        </router-link>
        <div class="symbol">{{ stock.symbol }}</div>
        <div v-if="stock.name" class="name">{{ stock.name }}</div>
      </div>

      <section class="section">
        <h2 class="section-title">Alerts</h2>
        <ul v-if="alerts.length > 0" class="alerts-list">
          <li v-for="a in alerts" :key="a.id" class="alert-item">
            <span class="alert-type">{{ alertTypeLabel(a.alertType) }}</span>
            <span v-if="a.payload" class="alert-payload">{{ a.payload }}</span>
            <span class="alert-date">{{ formatDate(a.createdAt) }}</span>
          </li>
        </ul>
        <p v-else class="no-alerts">No alerts yet.</p>
        <button type="button" class="btn" @click="showModal = true">
          Create Alert
        </button>
      </section>

      <CreateAlertModal
        v-model="showModal"
        :symbol="stock.symbol"
        @created="onAlertCreated"
      />
    </template>
    <div v-else class="error">Stock not found.</div>
  </div>
</template>

<script setup>
import { ref, onMounted, computed } from 'vue'
import { useRoute } from 'vue-router'
import { ArrowLeftIcon } from '@heroicons/vue/24/outline'
import CreateAlertModal from '../components/CreateAlertModal.vue'
import { api } from '../api/client'
import { useToastStore } from '../stores/toast'

const route = useRoute()
const toast = useToastStore()
const stock = ref(null)
const alerts = ref([])
const loading = ref(true)
const showModal = ref(false)

const symbol = computed(() => route.params.symbol)

const alertTypeLabels = {
  0: 'Price above',
  1: 'Price below',
  2: 'Percent change'
}

function alertTypeLabel(type) {
  return alertTypeLabels[type] ?? 'Alert'
}

function formatDate(iso) {
  if (!iso) return ''
  const d = new Date(iso)
  return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })
}

async function loadAlerts() {
  try {
    const res = await api.alerts.getBySymbol(symbol.value)
    alerts.value = res?.items ?? []
  } catch {
    alerts.value = []
  }
}

onMounted(async () => {
  try {
    stock.value = await api.stocks.get(symbol.value)
    if (stock.value) await loadAlerts()
  } catch (e) {
    toast.error(e.message || 'Failed to load stock')
  } finally {
    loading.value = false
  }
})

function onAlertCreated() {
  toast.success('Alert created')
  loadAlerts()
}
</script>

<style scoped>
.header {
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid rgba(255, 255, 255, 0.08);
}

.back {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  width: 44px;
  height: 44px;
  margin-bottom: 8px;
  color: var(--text-muted);
  border-radius: 10px;
}

.back:hover {
  color: var(--aqua);
  background: var(--aqua-dim);
}

.back-icon {
  width: 24px;
  height: 24px;
}

.symbol {
  font-size: 28px;
  font-weight: 700;
  color: var(--text);
}

.name {
  font-size: 15px;
  color: var(--text-muted);
  margin-top: 4px;
}

.section {
  margin-bottom: 24px;
}

.section-title {
  margin: 0 0 12px;
  font-size: 16px;
  font-weight: 500;
  color: var(--text-muted);
}

.alerts-list {
  list-style: none;
  margin: 0 0 16px;
  padding: 0;
}

.alert-item {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  padding: 12px 16px;
  background: var(--bg-card);
  border-radius: 8px;
  margin-bottom: 8px;
  font-size: 14px;
}

.alert-type {
  font-weight: 500;
  color: var(--aqua);
}

.alert-payload {
  color: var(--text-muted);
}

.alert-date {
  margin-left: auto;
  color: var(--text-muted);
  font-size: 13px;
}

.no-alerts {
  margin: 0 0 16px;
  color: var(--text-muted);
  font-size: 14px;
}

.btn {
  min-height: 48px;
  padding: 0 24px;
  background: var(--aqua);
  color: var(--bg-dark);
  border: none;
  border-radius: 8px;
  font-size: 16px;
  font-weight: 600;
  cursor: pointer;
}

.loading,
.error {
  padding: 32px 0;
  text-align: center;
  color: var(--text-muted);
}

.error {
  color: #f87171;
}
</style>
