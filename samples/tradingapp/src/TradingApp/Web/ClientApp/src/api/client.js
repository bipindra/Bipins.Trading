const base = '/api'

async function request(path, options = {}) {
  const url = path.startsWith('http') ? path : `${base}${path}`
  const res = await fetch(url, {
    ...options,
    headers: { 'Content-Type': 'application/json', ...options.headers }
  })
  if (!res.ok) {
    const text = await res.text()
    let err = { message: res.statusText }
    try {
      if (text && text.trim()) err = JSON.parse(text)
    } catch (_) { /* use statusText */ }
    const message = err.message || err.Message || text || res.statusText
    console.error(`API error ${res.status} ${url}:`, message)
    throw new Error(message)
  }
  const contentType = res.headers.get('content-type') || ''
  if (res.status === 204 || !contentType.includes('application/json')) return null
  const text = await res.text()
  if (!text || !text.trim()) return null
  return JSON.parse(text)
}

export const api = {
  stocks: {
    search(q) {
      return request(`/stocks/search?q=${encodeURIComponent(q || '')}`)
    },
    get(symbol) {
      return request(`/stocks/${encodeURIComponent(symbol)}`)
    }
  },
  watchlist: {
    get() {
      return request('/watchlist')
    },
    add(symbol) {
      return request('/watchlist', { method: 'POST', body: JSON.stringify({ symbol }) })
    },
    remove(symbol) {
      return request(`/watchlist/${encodeURIComponent(symbol)}`, { method: 'DELETE' })
    }
  },
  settings: {
    getAlpaca() {
      return request('/settings/alpaca')
    },
    saveAlpaca(data) {
      return request('/settings/alpaca', { method: 'POST', body: JSON.stringify(data) })
    }
  },
  alerts: {
    getBySymbol(symbol) {
      return request(`/alerts?symbol=${encodeURIComponent(symbol || '')}`)
    },
    create(data) {
      return request('/alerts', { method: 'POST', body: JSON.stringify(data) })
    },
    delete(id) {
      return request(`/alerts/${id}`, { method: 'DELETE' })
    }
  },
  notifications: {
    get(limit = 50, unreadOnly = false) {
      const params = new URLSearchParams({ limit: String(limit) })
      if (unreadOnly) params.set('unreadOnly', 'true')
      return request(`/notifications?${params}`)
    },
    markRead(id) {
      return request(`/notifications/${id}/read`, { method: 'PATCH' })
    }
  },
  trades: {
    execute(data) {
      return request('/trades', { method: 'POST', body: JSON.stringify(data) })
    },
    getIntradayBars(symbol, timeframe = '1Min') {
      return request(`/trades/${encodeURIComponent(symbol)}/intraday?timeframe=${encodeURIComponent(timeframe)}`)
    }
  },
  signals: {
    get(symbol, strategy = null) {
      const url = strategy
        ? `/signals/${encodeURIComponent(symbol)}?strategy=${encodeURIComponent(strategy)}`
        : `/signals/${encodeURIComponent(symbol)}`
      return request(url)
    }
  },
  activityLogs: {
    get(limit = 100, category = null) {
      const params = new URLSearchParams({ limit: String(limit) })
      if (category) params.set('category', category)
      return request(`/activity-logs?${params}`)
    },
    deleteAll() {
      return request('/activity-logs', { method: 'DELETE' })
    }
  }
}
