<template>
  <div class="page">
    <h1 class="page-title">Watchlist</h1>
    <div v-if="loading" class="loading">Loading...</div>
    <div v-else-if="items.length === 0" class="empty">
      <p>No stocks in your watchlist.</p>
      <router-link to="/search" class="link">Search to add stocks</router-link>
    </div>
    <StockList
      v-else
      :items="items"
      show-remove
      @select="goToStock"
      @remove="remove"
    />
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import * as signalR from '@microsoft/signalr'
import StockList from '../components/StockList.vue'
import { api } from '../api/client'
import { useToastStore } from '../stores/toast'

const router = useRouter()
const toast = useToastStore()
const items = ref([])
const loading = ref(true)
let priceConnection = null

onMounted(async () => {
  try {
    const res = await api.watchlist.get()
    items.value = res?.items ?? []
    startPriceUpdates()
  } catch (e) {
    toast.error(e.message || 'Failed to load watchlist')
  } finally {
    loading.value = false
  }
})

onUnmounted(() => {
  if (priceConnection) {
    priceConnection.stop()
    priceConnection = null
  }
})

function startPriceUpdates() {
  if (items.value.length === 0) return
  const baseUrl = window.location.origin
  priceConnection = new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/watchlist-price`)
    .withAutomaticReconnect()
    .build()
  priceConnection.on('PriceUpdate', (symbol, price) => {
    items.value = items.value.map((i) =>
      (i.symbol?.toUpperCase() === symbol?.toUpperCase()) ? { ...i, latestPrice: price } : i
    )
  })
  priceConnection.onreconnected(() => refreshSubscription())
  priceConnection.start()
    .then(() => refreshSubscription())
    .catch((err) => {
      console.warn('SignalR watchlist price connection failed:', err)
    })
}

function refreshSubscription() {
  if (priceConnection?.state === signalR.HubConnectionState.Connected && items.value.length > 0)
    priceConnection.invoke('Subscribe', items.value.map((i) => i.symbol).filter(Boolean)).catch(() => {})
}

function goToStock(item) {
  router.push(`/stock/${encodeURIComponent(item.symbol)}`)
}

async function remove(item) {
  try {
    await api.watchlist.remove(item.symbol)
    items.value = items.value.filter((i) => i.symbol !== item.symbol)
    refreshSubscription()
    toast.success('Removed from watchlist')
  } catch (e) {
    toast.error(e.message || 'Failed to remove')
  }
}
</script>

<style scoped>
.page-title {
  margin: 0 0 16px;
  font-size: 24px;
  font-weight: 600;
}

.loading,
.empty {
  padding: 32px 0;
  text-align: center;
  color: var(--text-muted);
}

.empty .link {
  display: inline-block;
  margin-top: 12px;
  color: var(--aqua);
  text-decoration: none;
}

.empty .link:hover {
  text-decoration: underline;
}
</style>
