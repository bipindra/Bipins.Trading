<template>
  <Transition name="modal">
    <div v-if="modelValue" class="modal-backdrop" @click.self="$emit('update:modelValue', false)">
      <div class="modal">
        <h3 class="modal-title">Create Alert</h3>
        <p class="modal-subtitle">{{ symbol }}</p>
        <div class="form">
          <label class="label">Alert type</label>
          <select v-model="form.alertType" class="select">
            <option value="0">Price above</option>
            <option value="1">Price below</option>
            <option value="2">Percent change</option>
            <option value="3">RSI oversold (buy signal)</option>
            <option value="4">RSI overbought (sell signal)</option>
          </select>
          <label class="label">Payload</label>
          <input v-model="form.payload" type="text" class="input" :placeholder="payloadPlaceholder" />
          <p v-if="isRsiType" class="hint">Optional: period (default 14), or "period,oversold,overbought" e.g. 14,30,70</p>
        </div>
        <div class="modal-actions">
          <button type="button" class="btn secondary" @click="$emit('update:modelValue', false)">
            Cancel
          </button>
          <button type="button" class="btn primary" :disabled="sending" @click="submit">
            {{ sending ? 'Creating...' : 'Create' }}
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, watch, computed } from 'vue'
import { api } from '../api/client'

const props = defineProps({
  modelValue: Boolean,
  symbol: { type: String, default: '' }
})
const emit = defineEmits(['update:modelValue', 'created'])

const form = ref({ alertType: '0', payload: '' })
const sending = ref(false)

const isRsiType = computed(() => form.value.alertType === '3' || form.value.alertType === '4')
const payloadPlaceholder = computed(() =>
  isRsiType.value ? 'e.g. 14 or 14,30,70' : 'e.g. 150.00')

watch(() => props.modelValue, (v) => {
  if (v) {
    form.value = { alertType: '0', payload: '' }
  }
})

async function submit() {
  sending.value = true
  try {
    await api.alerts.create({
      symbol: props.symbol,
      alertType: parseInt(form.value.alertType, 10),
      payload: form.value.payload || null
    })
    emit('created')
    emit('update:modelValue', false)
  } catch (e) {
    console.error(e)
  } finally {
    sending.value = false
  }
}
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0, 0, 0, 0.6);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 300;
  padding: 16px;
}

.modal {
  background: var(--bg-card);
  border-radius: var(--radius);
  padding: 24px;
  width: 100%;
  max-width: 360px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.modal-title {
  margin: 0 0 4px;
  font-size: 18px;
}

.modal-subtitle {
  margin: 0 0 20px;
  color: var(--text-muted);
  font-size: 14px;
}

.hint {
  margin: 4px 0 0;
  font-size: 12px;
  color: var(--text-muted);
}

.form {
  display: flex;
  flex-direction: column;
  gap: 8px;
  margin-bottom: 24px;
}

.label {
  font-size: 13px;
  color: var(--text-muted);
}

.select,
.input {
  padding: 12px;
  background: var(--bg-dark);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: 8px;
  color: var(--text);
  font-size: 16px;
}

.modal-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
}

.btn {
  min-height: 44px;
  padding: 0 20px;
  border-radius: 8px;
  font-size: 15px;
  font-weight: 500;
  cursor: pointer;
  border: none;
}

.btn.primary {
  background: var(--aqua);
  color: var(--bg-dark);
}

.btn.primary:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn.secondary {
  background: rgba(255, 255, 255, 0.1);
  color: var(--text);
}

.modal-enter-active,
.modal-leave-active {
  transition: opacity 0.2s;
}
.modal-enter-from,
.modal-leave-to {
  opacity: 0;
}
.modal-enter-active .modal,
.modal-leave-active .modal {
  transition: transform 0.2s;
}
.modal-enter-from .modal,
.modal-leave-to .modal {
  transform: scale(0.95);
}
</style>
