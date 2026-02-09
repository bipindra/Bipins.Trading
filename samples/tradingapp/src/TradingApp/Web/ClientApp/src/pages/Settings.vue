<template>
  <div class="page">
    <h1 class="page-title">Settings</h1>
    <h2 class="section-title">Alpaca API</h2>
    <form class="form" @submit.prevent="save">
      <label class="label">API Key</label>
      <input v-model="form.apiKey" type="text" class="input" placeholder="Your Alpaca API key" />
      <label class="label">API Secret</label>
      <input
        v-model="form.apiSecret"
        type="password"
        class="input"
        placeholder="Leave blank to keep existing"
      />
      <p v-if="hasExistingSecret" class="hint">Existing secret is stored; enter a new value only to change it.</p>
      <label class="label">Base URL</label>
      <input
        v-model="form.baseUrl"
        type="url"
        class="input"
        placeholder="https://api.alpaca.markets"
      />
      <button type="submit" class="btn" :disabled="saving">
        {{ saving ? 'Saving...' : 'Save' }}
      </button>
    </form>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { api } from '../api/client'
import { useToastStore } from '../stores/toast'

const toast = useToastStore()
const form = ref({
  apiKey: '',
  apiSecret: '',
  baseUrl: 'https://api.alpaca.markets'
})
const hasExistingSecret = ref(false)
const saving = ref(false)

onMounted(async () => {
  try {
    const res = await api.settings.getAlpaca()
    if (res?.apiKey) form.value.apiKey = res.apiKey
    if (res?.baseUrl) form.value.baseUrl = res.baseUrl
    if (res?.apiSecretMasked) hasExistingSecret.value = true
  } catch (e) {
    toast.error(e.message || 'Failed to load settings')
  }
})

async function save() {
  if (!form.value.apiKey?.trim() || !form.value.baseUrl?.trim()) {
    toast.error('API Key and Base URL are required')
    return
  }
  if (!hasExistingSecret.value && !form.value.apiSecret?.trim()) {
    toast.error('API Secret is required when saving for the first time')
    return
  }
  saving.value = true
  try {
    await api.settings.saveAlpaca({
      apiKey: form.value.apiKey.trim(),
      apiSecret: form.value.apiSecret || undefined,
      baseUrl: form.value.baseUrl.trim()
    })
    toast.success('Settings saved')
    hasExistingSecret.value = true
    form.value.apiSecret = ''
  } catch (e) {
    toast.error(e.message || 'Failed to save')
  } finally {
    saving.value = false
  }
}
</script>

<style scoped>
.page-title {
  margin: 0 0 24px;
  font-size: 24px;
  font-weight: 600;
}

.section-title {
  margin: 0 0 12px;
  font-size: 16px;
  font-weight: 500;
  color: var(--text-muted);
}

.form {
  display: flex;
  flex-direction: column;
  gap: 12px;
  max-width: 400px;
}

.label {
  font-size: 14px;
  color: var(--text-muted);
}

.input {
  padding: 14px 16px;
  background: var(--bg-card);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  color: var(--text);
  font-size: 16px;
}

.input:focus {
  outline: none;
  border-color: var(--aqua);
  box-shadow: 0 0 0 2px var(--aqua-dim);
}

.hint {
  margin: -4px 0 0;
  font-size: 13px;
  color: var(--text-muted);
}

.btn {
  margin-top: 8px;
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

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}
</style>
