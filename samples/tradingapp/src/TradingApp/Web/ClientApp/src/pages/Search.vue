<template>
  <div class="page">
    <div class="search-sticky">
      <SearchBar v-model="query" />
    </div>
    <div v-if="loading" class="loading">Searching...</div>
    <div v-else-if="query && !loading && results.length === 0" class="empty">
      No results for "{{ query }}".
    </div>
    <StockList
      v-else-if="results.length > 0"
      :items="results"
      @select="addAndGoHome"
    />
    <div v-else class="hint">
      Type a company name or symbol to search (e.g. Apple, AAPL).
    </div>
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import SearchBar from '../components/SearchBar.vue'
import StockList from '../components/StockList.vue'
import { api } from '../api/client'
import { useToastStore } from '../stores/toast'

const router = useRouter()
const toast = useToastStore()
const query = ref('')
const results = ref([])
const loading = ref(false)

let debounceTimer
watch(query, (q) => {
  clearTimeout(debounceTimer)
  const trimmed = (q || '').trim()
  if (!trimmed) {
    results.value = []
    loading.value = false
    return
  }
  loading.value = true
  debounceTimer = setTimeout(async () => {
    try {
      const res = await api.stocks.search(trimmed)
      const list = res?.items ?? res?.Items ?? []
      results.value = list.map((i) => ({
        symbol: i.symbol ?? i.Symbol ?? '',
        name: i.name ?? i.Name ?? ''
      }))
    } catch (e) {
      toast.error(e.message || 'Search failed')
      results.value = []
    } finally {
      loading.value = false
    }
  }, 300)
})

async function addAndGoHome(item) {
  try {
    await api.watchlist.add(item.symbol)
    toast.success(`Added ${item.symbol} to watchlist`)
    router.push('/')
  } catch (e) {
    toast.error(e.message || 'Failed to add')
  }
}
</script>

<style scoped>
.search-sticky {
  position: sticky;
  top: 0;
  z-index: 10;
  background: var(--bg-dark);
  padding-bottom: 8px;
}

.loading,
.empty,
.hint {
  padding: 24px 0;
  text-align: center;
  color: var(--text-muted);
  font-size: 15px;
}

.hint {
  padding: 48px 16px;
}
</style>
