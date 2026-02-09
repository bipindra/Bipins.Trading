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
        <div v-if="stock.latestPrice != null" class="price">${{ Number(stock.latestPrice).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 }) }}</div>
      </div>

      <section class="section">
        <div class="chart-wrapper">
          <LineChart :data="intradayBars" :width="400" :height="120" />
        </div>
      </section>

      <section class="section">
        <div class="trade-buttons">
          <button type="button" class="btn btn-buy" @click="openTradeModal('buy')">
            Buy
          </button>
          <button type="button" class="btn btn-sell" @click="openTradeModal('sell')">
            Sell
          </button>
        </div>
      </section>

      <section class="section">
        <h2 class="section-title">Trading Signals</h2>
        <div v-if="signalsLoading" class="loading-signals">Loading signals...</div>
        <ul v-else-if="signals.length > 0" class="signals-list">
          <li v-for="s in signals" :key="`${s.strategy}-${s.time}`" class="signal-item">
            <div class="signal-header">
              <span class="signal-strategy">{{ s.strategy }}</span>
              <span class="signal-type" :class="getSignalClass(s.signalType)">{{ formatSignalType(s.signalType) }}</span>
            </div>
            <div v-if="s.reason" class="signal-reason">{{ s.reason }}</div>
            <div class="signal-footer">
              <span v-if="s.price" class="signal-price">${{ Number(s.price).toFixed(2) }}</span>
              <span class="signal-time">{{ formatTime(s.time) }}</span>
              <button
                v-if="isEntrySignal(s.signalType)"
                type="button"
                class="btn-signal"
                :class="getSignalButtonClass(s.signalType)"
                @click="tradeOnSignal(s)"
              >
                Trade on Signal
              </button>
            </div>
          </li>
        </ul>
        <p v-else class="no-signals">No signals available.</p>
      </section>

      <section class="section">
        <h2 class="section-title">Alerts</h2>
        <ul v-if="alerts.length > 0" class="alerts-list">
          <li v-for="a in alerts" :key="a.id" class="alert-item">
            <div class="alert-content">
              <span class="alert-type">{{ alertTypeLabel(a.alertType) }}</span>
              <span v-if="a.payload" class="alert-payload">{{ a.payload }}</span>
              <span class="alert-date">{{ formatDate(a.createdAt) }}</span>
            </div>
            <button
              type="button"
              class="btn-delete"
              @click="deleteAlert(a.id)"
              :disabled="deleting === a.id"
              aria-label="Delete alert"
            >
              {{ deleting === a.id ? '...' : '×' }}
            </button>
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

      <TradeModal
        v-model="showTradeModal"
        :symbol="stock.symbol"
        :side="tradeSide"
        :current-price="stock.latestPrice || 0"
        :signal="selectedSignal"
        @executed="onTradeExecuted"
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
import TradeModal from '../components/TradeModal.vue'
import LineChart from '../components/LineChart.vue'
import { api } from '../api/client'
import { useToastStore } from '../stores/toast'

const route = useRoute()
const toast = useToastStore()
const stock = ref(null)
const alerts = ref([])
const signals = ref([])
const intradayBars = ref([])
const loading = ref(true)
const signalsLoading = ref(false)
const showModal = ref(false)
const showTradeModal = ref(false)
const tradeSide = ref('buy')
const selectedSignal = ref(null)
const deleting = ref(null)

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

async function loadIntradayBars() {
  try {
    const res = await api.trades.getIntradayBars(symbol.value, '1Min')
    if (res?.bars) {
      intradayBars.value = res.bars.map(b => b.close)
    }
  } catch (e) {
    console.error('Failed to load intraday bars:', e)
    intradayBars.value = []
  }
}

async function loadSignals() {
  signalsLoading.value = true
  try {
    const res = await api.signals.get(symbol.value)
    if (res?.signals) {
      signals.value = res.signals
    }
  } catch (e) {
    console.error('Failed to load signals:', e)
    signals.value = []
  } finally {
    signalsLoading.value = false
  }
}

function formatSignalType(type) {
  const map = {
    'EntryLong': 'Buy Signal',
    'EntryShort': 'Sell Signal',
    'ExitLong': 'Exit Long',
    'ExitShort': 'Exit Short',
    'Hold': 'Hold'
  }
  return map[type] || type
}

function getSignalClass(type) {
  if (type === 'EntryLong' || type === 'ExitShort') return 'signal-buy'
  if (type === 'EntryShort' || type === 'ExitLong') return 'signal-sell'
  return 'signal-hold'
}

function getSignalButtonClass(type) {
  if (type === 'EntryLong') return 'btn-signal-buy'
  if (type === 'EntryShort') return 'btn-signal-sell'
  return ''
}

function isEntrySignal(type) {
  return type === 'EntryLong' || type === 'EntryShort'
}

function formatTime(iso) {
  if (!iso) return ''
  const d = new Date(iso)
  return d.toLocaleString(undefined, { month: 'short', day: 'numeric', hour: '2-digit', minute: '2-digit' })
}

function tradeOnSignal(signal) {
  selectedSignal.value = signal
  if (signal.signalType === 'EntryLong') {
    tradeSide.value = 'buy'
  } else if (signal.signalType === 'EntryShort') {
    tradeSide.value = 'sell'
  }
  showTradeModal.value = true
}

onMounted(async () => {
  try {
    stock.value = await api.stocks.get(symbol.value)
    if (stock.value) {
      await Promise.all([loadAlerts(), loadIntradayBars(), loadSignals()])
    }
  } catch (e) {
    toast.error(e.message || 'Failed to load stock')
  } finally {
    loading.value = false
  }
})

async function onAlertCreated() {
  toast.success('Alert created')
  await loadAlerts()
}

async function deleteAlert(id) {
  if (!confirm('Are you sure you want to delete this alert?')) return
  deleting.value = id
  try {
    await api.alerts.delete(id)
    toast.success('Alert deleted')
    await loadAlerts()
  } catch (e) {
    toast.error(e.message || 'Failed to delete alert')
  } finally {
    deleting.value = null
  }
}

function openTradeModal(side) {
  tradeSide.value = side
  showTradeModal.value = true
}

function onTradeExecuted() {
  // Optionally reload data or show confirmation
  selectedSignal.value = null
  loadIntradayBars()
  loadSignals()
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

.price {
  font-size: 24px;
  font-weight: 700;
  color: var(--aqua);
  margin-top: 8px;
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
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  padding: 12px 16px;
  background: var(--bg-card);
  border-radius: 8px;
  margin-bottom: 8px;
  font-size: 14px;
}

.alert-content {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 8px;
  flex: 1;
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

.btn-delete {
  width: 32px;
  height: 32px;
  padding: 0;
  background: rgba(255, 71, 71, 0.1);
  color: #ff4747;
  border: 1px solid rgba(255, 71, 71, 0.3);
  border-radius: 6px;
  font-size: 20px;
  font-weight: 600;
  line-height: 1;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.btn-delete:hover:not(:disabled) {
  background: rgba(255, 71, 71, 0.2);
  border-color: rgba(255, 71, 71, 0.5);
}

.btn-delete:disabled {
  opacity: 0.5;
  cursor: not-allowed;
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

.chart-wrapper {
  background: var(--bg-card);
  border-radius: 8px;
  padding: 16px;
  height: 140px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.trade-buttons {
  display: flex;
  gap: 12px;
}

.btn-buy {
  flex: 1;
  background: var(--aqua);
  color: var(--bg-dark);
}

.btn-buy:hover {
  background: rgba(0, 217, 255, 0.9);
}

.btn-sell {
  flex: 1;
  background: rgba(255, 71, 71, 0.2);
  color: #ff4747;
  border: 1px solid rgba(255, 71, 71, 0.3);
}

.btn-sell:hover {
  background: rgba(255, 71, 71, 0.3);
}

.loading-signals {
  padding: 16px;
  text-align: center;
  color: var(--text-muted);
  font-size: 14px;
}

.signals-list {
  list-style: none;
  margin: 0 0 16px;
  padding: 0;
}

.signal-item {
  padding: 16px;
  background: var(--bg-card);
  border-radius: 8px;
  margin-bottom: 12px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.signal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.signal-strategy {
  font-weight: 600;
  font-size: 14px;
  color: var(--text);
}

.signal-type {
  padding: 4px 10px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
}

.signal-type.signal-buy {
  background: rgba(0, 217, 255, 0.2);
  color: var(--aqua);
}

.signal-type.signal-sell {
  background: rgba(255, 71, 71, 0.2);
  color: #ff4747;
}

.signal-type.signal-hold {
  background: rgba(255, 255, 255, 0.1);
  color: var(--text-muted);
}

.signal-reason {
  font-size: 13px;
  color: var(--text-muted);
  margin-bottom: 8px;
}

.signal-footer {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  flex-wrap: wrap;
}

.signal-price {
  font-weight: 600;
  color: var(--text);
  font-size: 14px;
}

.signal-time {
  font-size: 12px;
  color: var(--text-muted);
  margin-left: auto;
}

.btn-signal {
  padding: 6px 12px;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
  border: none;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-signal-buy {
  background: var(--aqua);
  color: var(--bg-dark);
}

.btn-signal-buy:hover {
  background: rgba(0, 217, 255, 0.9);
}

.btn-signal-sell {
  background: rgba(255, 71, 71, 0.2);
  color: #ff4747;
  border: 1px solid rgba(255, 71, 71, 0.3);
}

.btn-signal-sell:hover {
  background: rgba(255, 71, 71, 0.3);
}

.no-signals {
  margin: 0;
  color: var(--text-muted);
  font-size: 14px;
  padding: 16px;
  text-align: center;
}
</style>
