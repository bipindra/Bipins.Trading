<template>
  <Transition name="toast">
    <div v-if="store.visible" class="toast" :class="store.type">
      {{ store.message }}
    </div>
  </Transition>
</template>

<script setup>
import { useToastStore } from '../stores/toast'

const store = useToastStore()
</script>

<style scoped>
.toast {
  position: fixed;
  bottom: calc(72px + var(--safe-bottom) + 16px);
  left: 50%;
  transform: translateX(-50%);
  max-width: calc(100% - 32px);
  padding: 12px 20px;
  border-radius: var(--radius);
  background: var(--bg-card);
  border: 1px solid rgba(255, 255, 255, 0.1);
  color: var(--text);
  font-size: 14px;
  z-index: 200;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.4);
}

.toast.success {
  border-color: var(--aqua);
  background: var(--aqua-dim);
}

.toast.error {
  border-color: #f87171;
  background: rgba(248, 113, 113, 0.15);
}

.toast-enter-active,
.toast-leave-active {
  transition: opacity 0.2s, transform 0.2s;
}
.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translateX(-50%) translateY(8px);
}
</style>
