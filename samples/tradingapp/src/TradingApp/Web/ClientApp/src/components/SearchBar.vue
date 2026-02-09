<template>
  <div class="search-bar">
    <MagnifyingGlassIcon class="search-icon" />
    <input
      v-model="localQuery"
      type="search"
      placeholder="Search stocks..."
      class="input"
      autocomplete="off"
      aria-label="Search stocks"
      @input="onInput"
    />
  </div>
</template>

<script setup>
import { ref, watch } from 'vue'
import { MagnifyingGlassIcon } from '@heroicons/vue/24/outline'

const props = defineProps({ modelValue: { type: String, default: '' } })
const emit = defineEmits(['update:modelValue'])

const localQuery = ref(props.modelValue)

watch(() => props.modelValue, (v) => { localQuery.value = v })

function onInput() {
  emit('update:modelValue', localQuery.value)
}
</script>

<style scoped>
.search-bar {
  display: flex;
  align-items: center;
  gap: 10px;
  background: var(--bg-card);
  border: 1px solid rgba(255, 255, 255, 0.1);
  border-radius: var(--radius);
  padding: 12px 16px;
  margin-bottom: 12px;
}

.search-icon {
  width: 22px;
  height: 22px;
  color: var(--text-muted);
  flex-shrink: 0;
}

.input {
  flex: 1;
  background: none;
  border: none;
  color: var(--text);
  font-size: 16px;
  outline: none;
}

.input::placeholder {
  color: var(--text-muted);
}
</style>
