<template>
  <Transition name="modal">
    <div v-if="modelValue" class="modal-backdrop" @click.self="$emit('update:modelValue', false)">
      <div class="modal">
        <h3 class="modal-title">Create Alert</h3>
        <p class="modal-subtitle">{{ symbol }}</p>
        
        <!-- Step 1: Configure Alert -->
        <div v-if="step === 'configure'" class="form">
          <label class="label">Alert type</label>
          <select v-model="form.alertType" class="select">
            <option value="0">Price above</option>
            <option value="1">Price below</option>
            <option value="2">Percent change</option>
            <option value="3">RSI oversold (buy signal)</option>
            <option value="4">RSI overbought (sell signal)</option>
          </select>
          
          <template v-if="isRsiType">
            <label class="label">RSI Threshold</label>
            <input v-model.number="form.threshold" type="number" step="0.1" class="input" placeholder="e.g. 30" />
            <p class="hint">RSI value to trigger alert (default: 30 for oversold, 70 for overbought)</p>
            
            <label class="label">Comparison Type</label>
            <select v-model.number="form.comparisonType" class="select">
              <option :value="null">Default (Below/Above threshold)</option>
              <option :value="0">Above threshold</option>
              <option :value="1">Below threshold</option>
              <option :value="2">Crosses over threshold</option>
              <option :value="3">Crosses below threshold</option>
            </select>
            
            <label class="label">Timeframe</label>
            <select v-model="form.timeframe" class="select">
              <option value="">Default (1Day)</option>
              <option value="1Min">1 Minute</option>
              <option value="3Min">3 Minutes</option>
              <option value="5Min">5 Minutes</option>
              <option value="15Min">15 Minutes</option>
              <option value="1Hour">1 Hour</option>
              <option value="1Day">1 Day</option>
            </select>
            <p class="hint">Bar length for RSI calculation</p>
          </template>
          
          <template v-else>
            <label class="label">Threshold</label>
            <input v-model.number="form.threshold" type="number" step="0.01" class="input" :placeholder="thresholdPlaceholder" />
            
            <label class="label">Comparison Type</label>
            <select v-model.number="form.comparisonType" class="select">
              <option :value="null">Default (Above/Below)</option>
              <option :value="0">Above threshold</option>
              <option :value="1">Below threshold</option>
              <option :value="2">Crosses over threshold</option>
              <option :value="3">Crosses below threshold</option>
            </select>
          </template>
          
          <label class="label">Payload (Advanced)</label>
          <input v-model="form.payload" type="text" class="input" :placeholder="payloadPlaceholder" />
          <p v-if="isRsiType" class="hint">Optional: period (default 14), or "period,oversold,overbought" e.g. 14,30,70</p>
          
          <div class="divider"></div>
          
          <div class="section-header">
            <label class="label">Auto-Execute Order</label>
            <label class="toggle-switch">
              <input v-model="form.enableAutoExecute" type="checkbox" />
              <span class="toggle-slider"></span>
            </label>
          </div>
          <p class="hint">Automatically execute a buy/sell order when this alert triggers</p>
          
          <template v-if="form.enableAutoExecute">
            <label class="label">Order Side</label>
            <select v-model.number="form.orderSideOverride" class="select">
              <option :value="null">Default ({{ defaultOrderSide }})</option>
              <option :value="0">Buy</option>
              <option :value="1">Sell</option>
            </select>
            
            <label class="label">Order Type</label>
            <select v-model.number="form.orderType" class="select">
              <option :value="null">Market (default)</option>
              <option :value="0">Market</option>
              <option :value="1">Limit</option>
              <option :value="2">Stop</option>
              <option :value="3">Stop Limit</option>
            </select>
            
            <label class="label">Quantity</label>
            <input v-model.number="form.orderQuantity" type="number" step="1" min="1" class="input" placeholder="1" />
            
            <label v-if="form.orderType === 1 || form.orderType === 3" class="label">Limit Price ($)</label>
            <input v-if="form.orderType === 1 || form.orderType === 3" v-model.number="form.orderLimitPrice" type="number" step="0.01" min="0.01" class="input" placeholder="0.00" />
            
            <label v-if="form.orderType === 2 || form.orderType === 3" class="label">Stop Price ($)</label>
            <input v-if="form.orderType === 2 || form.orderType === 3" v-model.number="form.orderStopPrice" type="number" step="0.01" min="0.01" class="input" placeholder="0.00" />
            
            <label class="label">Time in Force</label>
            <select v-model.number="form.orderTimeInForce" class="select">
              <option :value="null">Day (default)</option>
              <option :value="3">Day</option>
              <option :value="0">Good Till Canceled (GTC)</option>
              <option :value="1">Immediate or Cancel (IOC)</option>
              <option :value="2">Fill or Kill (FOK)</option>
            </select>
          </template>
        </div>
        
        <!-- Step 2: Confirm Order (if auto-execute enabled) -->
        <div v-if="step === 'confirm' && form.enableAutoExecute" class="confirm">
          <h4 class="confirm-title">Confirm Order</h4>
          <p class="confirm-subtitle">This order will execute automatically when the alert triggers</p>
          <div class="confirm-details">
            <div class="detail-row">
              <span>Symbol:</span>
              <span>{{ symbol }}</span>
            </div>
            <div class="detail-row">
              <span>Side:</span>
              <span class="side-badge" :class="orderSideClass">{{ orderSideDisplay }}</span>
            </div>
            <div class="detail-row">
              <span>Order Type:</span>
              <span>{{ orderTypeDisplay }}</span>
            </div>
            <div v-if="form.orderLimitPrice || (form.orderType === 1 || form.orderType === 3)" class="detail-row">
              <span>Limit Price:</span>
              <span>${{ (form.orderLimitPrice || 0).toFixed(2) }}</span>
            </div>
            <div v-if="form.orderStopPrice || (form.orderType === 2 || form.orderType === 3)" class="detail-row">
              <span>Stop Price:</span>
              <span>${{ (form.orderStopPrice || 0).toFixed(2) }}</span>
            </div>
            <div class="detail-row">
              <span>Quantity:</span>
              <span>{{ form.orderQuantity || 1 }}</span>
            </div>
            <div class="detail-row">
              <span>Time in Force:</span>
              <span>{{ timeInForceDisplay }}</span>
            </div>
            <div class="detail-row total">
              <span>Estimated Cost:</span>
              <span>${{ estimatedCost.toFixed(2) }}</span>
            </div>
          </div>
        </div>
        
        <!-- Step 2: Confirm Alert (if auto-execute disabled) -->
        <div v-if="step === 'confirm' && !form.enableAutoExecute" class="confirm">
          <h4 class="confirm-title">Confirm Alert</h4>
          <div class="confirm-details">
            <div class="detail-row">
              <span>Symbol:</span>
              <span>{{ symbol }}</span>
            </div>
            <div class="detail-row">
              <span>Alert Type:</span>
              <span>{{ alertTypeDisplay }}</span>
            </div>
            <div v-if="form.threshold" class="detail-row">
              <span>Threshold:</span>
              <span>{{ form.threshold }}</span>
            </div>
            <div v-if="form.timeframe" class="detail-row">
              <span>Timeframe:</span>
              <span>{{ form.timeframe }}</span>
            </div>
            <div class="detail-row">
              <span>Auto-Execute:</span>
              <span>Disabled</span>
            </div>
          </div>
        </div>
        
        <div class="modal-actions">
          <button v-if="step === 'configure'" type="button" class="btn secondary" @click="$emit('update:modelValue', false)">
            Cancel
          </button>
          <button v-if="step === 'confirm'" type="button" class="btn secondary" @click="step = 'configure'">
            Back
          </button>
          <button v-if="step === 'configure'" type="button" class="btn primary" :disabled="!isValid" @click="goToConfirm">
            {{ form.enableAutoExecute ? 'Review Order' : 'Review Alert' }}
          </button>
          <button v-if="step === 'confirm'" type="button" class="btn primary" :disabled="sending" @click="submit">
            {{ sending ? 'Creating...' : 'Create Alert' }}
          </button>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup>
import { ref, watch, computed } from 'vue'
import { api } from '../api/client'
import { useToastStore } from '../stores/toast'

const props = defineProps({
  modelValue: Boolean,
  symbol: { type: String, default: '' }
})
const emit = defineEmits(['update:modelValue', 'created'])

const toast = useToastStore()
const step = ref('configure') // 'configure' or 'confirm'
const form = ref({ 
  alertType: '0', 
  payload: '', 
  threshold: null,
  comparisonType: null,
  timeframe: '',
  enableAutoExecute: false,
  orderSideOverride: null,
  orderType: null,
  orderQuantity: null,
  orderLimitPrice: null,
  orderStopPrice: null,
  orderTimeInForce: null
})
const sending = ref(false)
const currentPrice = ref(0) // Will be passed from parent or fetched

const isRsiType = computed(() => form.value.alertType === '3' || form.value.alertType === '4')
const payloadPlaceholder = computed(() =>
  isRsiType.value ? 'e.g. 14 or 14,30,70' : 'e.g. 150.00')
const thresholdPlaceholder = computed(() => {
  if (form.value.alertType === '0') return 'e.g. 150.00'
  if (form.value.alertType === '1') return 'e.g. 150.00'
  return 'e.g. 5.0'
})
const defaultOrderSide = computed(() => {
  // Default: PriceAbove/RSIOversold -> Buy, PriceBelow/RSIOverbought -> Sell
  return (form.value.alertType === '0' || form.value.alertType === '3') ? 'Buy' : 'Sell'
})

const orderSideDisplay = computed(() => {
  if (form.value.orderSideOverride === 0) return 'BUY'
  if (form.value.orderSideOverride === 1) return 'SELL'
  return defaultOrderSide.value.toUpperCase()
})

const orderSideClass = computed(() => {
  const side = form.value.orderSideOverride !== null 
    ? (form.value.orderSideOverride === 0 ? 'buy' : 'sell')
    : (form.value.alertType === '0' || form.value.alertType === '3' ? 'buy' : 'sell')
  return side
})

const orderTypeDisplay = computed(() => {
  const map = { 0: 'Market', 1: 'Limit', 2: 'Stop', 3: 'Stop Limit' }
  return map[form.value.orderType] || 'Market'
})

const timeInForceDisplay = computed(() => {
  const map = { 0: 'GTC', 1: 'IOC', 2: 'FOK', 3: 'Day' }
  return map[form.value.orderTimeInForce] || 'Day'
})

const alertTypeDisplay = computed(() => {
  const map = {
    '0': 'Price Above',
    '1': 'Price Below',
    '2': 'Percent Change',
    '3': 'RSI Oversold',
    '4': 'RSI Overbought'
  }
  return map[form.value.alertType] || 'Unknown'
})

const estimatedCost = computed(() => {
  if (!form.value.enableAutoExecute) return 0
  const qty = form.value.orderQuantity || 1
  const price = form.value.orderLimitPrice || form.value.orderStopPrice || currentPrice.value || 0
  return qty * price
})

const isValid = computed(() => {
  if (!form.value.alertType) return false
  // Basic alert is valid even without threshold
  if (!form.value.enableAutoExecute) return true
  
  // If auto-execute is enabled, validate order fields
  if (form.value.enableAutoExecute) {
    // Quantity is required if auto-execute is enabled
    if (!form.value.orderQuantity || form.value.orderQuantity <= 0) return false
    
    // If order type is Limit or StopLimit, limit price is required
    if (form.value.orderType === 1 || form.value.orderType === 3) {
      if (!form.value.orderLimitPrice || form.value.orderLimitPrice <= 0) return false
    }
    // If order type is Stop or StopLimit, stop price is required
    if (form.value.orderType === 2 || form.value.orderType === 3) {
      if (!form.value.orderStopPrice || form.value.orderStopPrice <= 0) return false
    }
  }
  return true
})

function goToConfirm() {
  if (isValid.value) {
    step.value = 'confirm'
  }
}

watch(() => props.modelValue, (v) => {
  if (v) {
    step.value = 'configure'
    form.value = { 
      alertType: '0', 
      payload: '', 
      threshold: null,
      comparisonType: null,
      timeframe: '',
      enableAutoExecute: false,
      orderSideOverride: null,
      orderType: null,
      orderQuantity: null,
      orderLimitPrice: null,
      orderStopPrice: null,
      orderTimeInForce: null
    }
  }
})

async function submit() {
  console.log('Submitting alert:', form.value)
  if (!isValid.value) {
    toast.error('Please fill in all required fields')
    return
  }
  
  sending.value = true
  try {
    const alertData = {
      symbol: props.symbol,
      alertType: parseInt(form.value.alertType, 10),
      payload: form.value.payload || null,
      threshold: form.value.threshold || null,
      comparisonType: form.value.comparisonType !== null ? form.value.comparisonType : null,
      timeframe: form.value.timeframe || null,
      enableAutoExecute: form.value.enableAutoExecute || false,
      orderQuantity: form.value.orderQuantity || null,
      orderType: form.value.orderType !== null ? form.value.orderType : null,
      orderSideOverride: form.value.orderSideOverride !== null ? form.value.orderSideOverride : null,
      orderLimitPrice: form.value.orderLimitPrice || null,
      orderStopPrice: form.value.orderStopPrice || null,
      orderTimeInForce: form.value.orderTimeInForce !== null ? form.value.orderTimeInForce : null
    }
    console.log('Alert data to send:', alertData)
    await api.alerts.create(alertData)
    toast.success('Alert created successfully')
    emit('created')
    emit('update:modelValue', false)
  } catch (e) {
    console.error('Failed to create alert:', e)
    toast.error(e.message || 'Failed to create alert')
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
  max-height: 90vh;
  overflow-y: auto;
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

.divider {
  height: 1px;
  background: rgba(255, 255, 255, 0.1);
  margin: 20px 0;
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 8px;
}

.toggle-switch {
  position: relative;
  display: inline-block;
  width: 44px;
  height: 24px;
}

.toggle-switch input {
  opacity: 0;
  width: 0;
  height: 0;
}

.toggle-slider {
  position: absolute;
  cursor: pointer;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background-color: rgba(255, 255, 255, 0.2);
  transition: 0.3s;
  border-radius: 24px;
}

.toggle-slider:before {
  position: absolute;
  content: "";
  height: 18px;
  width: 18px;
  left: 3px;
  bottom: 3px;
  background-color: white;
  transition: 0.3s;
  border-radius: 50%;
}

.toggle-switch input:checked + .toggle-slider {
  background-color: var(--aqua);
}

.toggle-switch input:checked + .toggle-slider:before {
  transform: translateX(20px);
}

.confirm {
  margin-bottom: 24px;
}

.confirm-title {
  margin: 0 0 8px;
  font-size: 16px;
  font-weight: 600;
}

.confirm-subtitle {
  margin: 0 0 16px;
  font-size: 13px;
  color: var(--text-muted);
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
  color: var(--text-muted);
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
  color: var(--text);
}

.detail-row span:last-child {
  color: var(--text);
  font-weight: 500;
}

.side-badge {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
}

.side-badge.buy {
  background: rgba(0, 217, 255, 0.2);
  color: var(--aqua);
}

.side-badge.sell {
  background: rgba(255, 71, 71, 0.2);
  color: #ff4747;
}
</style>
