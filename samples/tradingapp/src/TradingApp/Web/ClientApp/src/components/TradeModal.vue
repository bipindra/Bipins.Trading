<template>
  <Transition name="modal">
    <div v-if="modelValue" class="modal-backdrop" @click.self="$emit('update:modelValue', false)">
      <div class="modal">
        <h3 class="modal-title">{{ side === 'buy' ? 'Buy' : 'Sell' }} {{ symbol }}</h3>
        
        <div v-if="signal" class="signal-info">
          <div class="signal-badge" :class="signal.signalType === 'EntryLong' ? 'signal-buy' : 'signal-sell'">
            <span class="signal-label">Trading on Signal:</span>
            <span class="signal-name">{{ signal.strategy }} - {{ formatSignalType(signal.signalType) }}</span>
          </div>
          <p v-if="signal.reason" class="signal-reason-text">{{ signal.reason }}</p>
        </div>
        
        <div v-if="step === 'order'" class="form">
          <label class="label">Order Type</label>
          <select v-model="form.orderType" class="select">
            <option value="Market">Market</option>
            <option value="Limit">Limit</option>
            <option value="Stop">Stop</option>
            <option value="StopLimit">Stop Limit</option>
          </select>

          <label v-if="form.orderType === 'Limit' || form.orderType === 'StopLimit'" class="label">
            Limit Price ($)
          </label>
          <input
            v-if="form.orderType === 'Limit' || form.orderType === 'StopLimit'"
            v-model.number="form.limitPrice"
            type="number"
            step="0.01"
            min="0.01"
            class="input"
            placeholder="0.00"
          />

          <label v-if="form.orderType === 'Stop' || form.orderType === 'StopLimit'" class="label">
            Stop Price ($)
          </label>
          <input
            v-if="form.orderType === 'Stop' || form.orderType === 'StopLimit'"
            v-model.number="form.stopPrice"
            type="number"
            step="0.01"
            min="0.01"
            class="input"
            placeholder="0.00"
          />

          <label class="label">Quantity</label>
          <input
            v-model.number="form.quantity"
            type="number"
            step="1"
            min="1"
            class="input"
            placeholder="1"
          />

          <label class="label">Time in Force</label>
          <select v-model="form.timeInForce" class="select">
            <option value="Day">Day</option>
            <option value="GTC">Good Till Canceled (GTC)</option>
            <option value="IOC">Immediate or Cancel (IOC)</option>
            <option value="FOK">Fill or Kill (FOK)</option>
          </select>

          <label v-if="form.timeInForce === 'GTC'" class="label">
            Good Till Date (Optional)
          </label>
          <input
            v-if="form.timeInForce === 'GTC'"
            v-model="form.expirationDate"
            type="date"
            :min="minDate"
            class="input"
          />

          <div class="cost-summary">
            <div class="cost-row">
              <span>Price:</span>
              <span>${{ displayPrice.toFixed(2) }}</span>
            </div>
            <div class="cost-row">
              <span>Quantity:</span>
              <span>{{ form.quantity || 0 }}</span>
            </div>
            <div class="cost-row total">
              <span>Estimated Cost:</span>
              <span>${{ estimatedCost.toFixed(2) }}</span>
            </div>
          </div>
        </div>

        <div v-if="step === 'confirm'" class="confirm">
          <h4 class="confirm-title">Confirm Order</h4>
          <div class="confirm-details">
            <div class="detail-row">
              <span>Symbol:</span>
              <span>{{ symbol }}</span>
            </div>
            <div class="detail-row">
              <span>Side:</span>
              <span class="side-badge" :class="side">{{ side.toUpperCase() }}</span>
            </div>
            <div class="detail-row">
              <span>Order Type:</span>
              <span>{{ form.orderType }}</span>
            </div>
            <div v-if="form.limitPrice" class="detail-row">
              <span>Limit Price:</span>
              <span>${{ form.limitPrice.toFixed(2) }}</span>
            </div>
            <div v-if="form.stopPrice" class="detail-row">
              <span>Stop Price:</span>
              <span>${{ form.stopPrice.toFixed(2) }}</span>
            </div>
            <div class="detail-row">
              <span>Quantity:</span>
              <span>{{ form.quantity }}</span>
            </div>
            <div class="detail-row">
              <span>Time in Force:</span>
              <span>{{ form.timeInForce }}</span>
            </div>
            <div v-if="form.expirationDate" class="detail-row">
              <span>Expiration Date:</span>
              <span>{{ formatDate(form.expirationDate) }}</span>
            </div>
            <div class="detail-row total">
              <span>Estimated Cost:</span>
              <span>${{ estimatedCost.toFixed(2) }}</span>
            </div>
          </div>
        </div>

        <div class="modal-actions">
          <button
            v-if="step === 'order'"
            type="button"
            class="btn secondary"
            @click="$emit('update:modelValue', false)"
          >
            Cancel
          </button>
          <button
            v-if="step === 'confirm'"
            type="button"
            class="btn secondary"
            @click="step = 'order'"
          >
            Back
          </button>
          <button
            v-if="step === 'order'"
            type="button"
            class="btn primary"
            :disabled="!isValid"
            @click="step = 'confirm'"
          >
            Review Order
          </button>
          <button
            v-if="step === 'confirm'"
            type="button"
            class="btn primary"
            :disabled="sending"
            @click="submit"
          >
            {{ sending ? 'Submitting...' : 'Confirm & Submit' }}
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, computed, watch } from 'vue'
import { api } from '../api/client'
import { useToastStore } from '../stores/toast'

const props = defineProps({
  modelValue: Boolean,
  symbol: { type: String, required: true },
  side: { type: String, required: true, validator: (v) => ['buy', 'sell'].includes(v) },
  currentPrice: { type: Number, default: 0 },
  signal: { type: Object, default: null }
})

const emit = defineEmits(['update:modelValue', 'executed'])

const toast = useToastStore()
const step = ref('order')
const sending = ref(false)

const form = ref({
  orderType: 'Market',
  quantity: 1,
  limitPrice: null,
  stopPrice: null,
  timeInForce: 'Day',
  expirationDate: null
})

const minDate = computed(() => {
  const tomorrow = new Date()
  tomorrow.setDate(tomorrow.getDate() + 1)
  return tomorrow.toISOString().split('T')[0]
})

const displayPrice = computed(() => {
  if (form.value.orderType === 'Market') {
    return props.currentPrice || 0
  }
  if (form.value.orderType === 'Limit' || form.value.orderType === 'StopLimit') {
    return form.value.limitPrice || props.currentPrice || 0
  }
  return props.currentPrice || 0
})

const estimatedCost = computed(() => {
  const qty = form.value.quantity || 0
  return displayPrice.value * qty
})

const isValid = computed(() => {
  if (!form.value.quantity || form.value.quantity <= 0) return false
  if (form.value.orderType === 'Limit' && !form.value.limitPrice) return false
  if (form.value.orderType === 'Stop' && !form.value.stopPrice) return false
  if (form.value.orderType === 'StopLimit' && (!form.value.limitPrice || !form.value.stopPrice)) return false
  return true
})

watch(() => props.modelValue, (v) => {
  if (v) {
    step.value = 'order'
    // Pre-fill form based on signal if provided
    if (props.signal) {
      const signalPrice = props.signal.price || props.currentPrice
      form.value = {
        orderType: 'Market',
        quantity: 1,
        limitPrice: signalPrice,
        stopPrice: null,
        timeInForce: 'Day',
        expirationDate: null
      }
    } else {
      form.value = {
        orderType: 'Market',
        quantity: 1,
        limitPrice: props.currentPrice || null,
        stopPrice: null,
        timeInForce: 'Day',
        expirationDate: null
      }
    }
  }
})

watch(() => props.currentPrice, (price) => {
  if (price && !form.value.limitPrice && form.value.orderType === 'Limit') {
    form.value.limitPrice = price
  }
})

function formatDate(dateStr) {
  if (!dateStr) return ''
  const d = new Date(dateStr)
  return d.toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' })
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

async function submit() {
  console.log('Submit called', { form: form.value, isValid: isValid.value })
  if (!isValid.value) {
    toast.error('Please fill in all required fields')
    return
  }
  
  sending.value = true
  try {
    // Map string values to enum values
    const sideMap = { 'buy': 0, 'sell': 1 } // OrderSide: Buy=0, Sell=1
    const orderTypeMap = { 'Market': 0, 'Limit': 1, 'Stop': 2, 'StopLimit': 3 } // OrderType enum
    const timeInForceMap = { 'Day': 3, 'GTC': 0, 'IOC': 1, 'FOK': 2 } // TimeInForce enum
    
    const sideValue = sideMap[props.side]
    const orderTypeValue = orderTypeMap[form.value.orderType]
    const timeInForceValue = timeInForceMap[form.value.timeInForce]
    
    if (sideValue === undefined) {
      throw new Error(`Invalid side: ${props.side}`)
    }
    if (orderTypeValue === undefined) {
      throw new Error(`Invalid order type: ${form.value.orderType}`)
    }
    if (timeInForceValue === undefined) {
      throw new Error(`Invalid time in force: ${form.value.timeInForce}`)
    }
    
    const tradeData = {
      symbol: props.symbol,
      side: sideValue,
      orderType: orderTypeValue,
      timeInForce: timeInForceValue,
      quantity: form.value.quantity,
      limitPrice: form.value.limitPrice || null,
      stopPrice: form.value.stopPrice || null,
      expirationDate: form.value.expirationDate || null
    }

    console.log('Submitting trade:', tradeData)
    await api.trades.execute(tradeData)
    toast.success(`${props.side === 'buy' ? 'Buy' : 'Sell'} order submitted successfully`)
    emit('executed')
    emit('update:modelValue', false)
  } catch (e) {
    console.error('Trade submission error:', e)
    toast.error(e.message || 'Failed to submit order')
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
  max-width: 420px;
  max-height: 90vh;
  overflow-y: auto;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.modal-title {
  margin: 0 0 20px;
  font-size: 20px;
  font-weight: 600;
}

.form {
  display: flex;
  flex-direction: column;
  gap: 12px;
  margin-bottom: 24px;
}

.label {
  font-size: 13px;
  color: var(--text-muted);
  font-weight: 500;
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

.select:focus,
.input:focus {
  outline: none;
  border-color: var(--aqua);
}

.cost-summary {
  margin-top: 16px;
  padding: 16px;
  background: var(--bg-dark);
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.cost-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 8px;
  font-size: 14px;
  color: var(--text-muted);
}

.cost-row:last-child {
  margin-bottom: 0;
}

.cost-row.total {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  font-size: 16px;
  font-weight: 600;
  color: var(--text);
}

.confirm {
  margin-bottom: 24px;
}

.confirm-title {
  margin: 0 0 16px;
  font-size: 16px;
  font-weight: 600;
}

.confirm-details {
  padding: 16px;
  background: var(--bg-dark);
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.detail-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 12px;
  font-size: 14px;
}

.detail-row:last-child {
  margin-bottom: 0;
}

.detail-row.total {
  margin-top: 12px;
  padding-top: 12px;
  border-top: 1px solid rgba(255, 255, 255, 0.1);
  font-size: 16px;
  font-weight: 600;
}

.side-badge {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
}

.side-badge.buy {
  background: rgba(0, 217, 255, 0.2);
  color: var(--aqua);
}

.side-badge.sell {
  background: rgba(255, 71, 71, 0.2);
  color: #ff4747;
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

.signal-info {
  margin-bottom: 20px;
  padding: 12px;
  background: var(--bg-dark);
  border-radius: 8px;
  border: 1px solid rgba(255, 255, 255, 0.1);
}

.signal-badge {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
}

.signal-badge.signal-buy {
  color: var(--aqua);
}

.signal-badge.signal-sell {
  color: #ff4747;
}

.signal-label {
  font-weight: 500;
  color: var(--text-muted);
}

.signal-name {
  font-weight: 600;
}

.signal-reason-text {
  margin: 8px 0 0;
  font-size: 13px;
  color: var(--text-muted);
  font-style: italic;
}
</style>
