<template>
  <ul class="stock-list">
    <li
      v-for="item in items"
      :key="item.symbol"
      class="stock-item"
      @click="$emit('select', item)"
    >
      <div class="symbol">{{ item.symbol }}</div>
      <div v-if="item.name" class="name">{{ item.name }}</div>
      <div v-if="priceDisplay(item)" class="price">{{ priceDisplay(item) }}</div>
      <button
        v-if="showRemove"
        type="button"
        class="remove"
        aria-label="Remove"
        @click.stop="$emit('remove', item)"
      >
        <XMarkIcon class="remove-icon" />
      </button>
    </li>
  </ul>
</template>

<script setup>
import { XMarkIcon } from '@heroicons/vue/24/outline'

defineProps({
  items: { type: Array, default: () => [] },
  showRemove: { type: Boolean, default: false }
})
defineEmits(['select', 'remove'])

function priceDisplay(item) {
  const p = item.latestPrice ?? item.latestprice
  if (p == null || typeof p !== 'number') return null
  return '$' + Number(p).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}
</script>

<style scoped>
.stock-list {
  list-style: none;
  margin: 0;
  padding: 0;
}

.stock-item {
  display: flex;
  align-items: center;
  gap: 12px;
  padding: 16px;
  background: var(--bg-card);
  border-radius: var(--radius);
  margin-bottom: 8px;
  cursor: pointer;
  min-height: 56px;
  transition: background 0.2s;
}

.stock-item:hover {
  background: rgba(255, 255, 255, 0.06);
}

.stock-item:active {
  background: var(--aqua-dim);
}

.symbol {
  font-weight: 600;
  font-size: 17px;
  color: var(--text);
}

.name {
  flex: 1;
  font-size: 14px;
  color: var(--text-muted);
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.price {
  flex-shrink: 0;
  font-size: 15px;
  font-weight: 600;
  color: var(--text);
}

.remove {
  flex-shrink: 0;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(255, 255, 255, 0.08);
  border: none;
  border-radius: 10px;
  color: var(--text-muted);
  cursor: pointer;
}

.remove:hover {
  color: #f87171;
  background: rgba(248, 113, 113, 0.15);
}

.remove-icon {
  width: 20px;
  height: 20px;
}
</style>
