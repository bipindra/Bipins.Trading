<template>
  <div class="app">
    <main class="main">
      <RouterView v-slot="{ Component }">
        <Transition name="fade" mode="out-in">
          <component :is="Component" />
        </Transition>
      </RouterView>
    </main>
    <BottomTabs />
    <Toast />
  </div>
</template>

<script setup>
import BottomTabs from './components/BottomTabs.vue'
import Toast from './components/Toast.vue'
</script>

<style>
:root {
  --bg-dark: #0d1117;
  --bg-card: #161b22;
  --aqua: #00d4aa;
  --aqua-dim: rgba(0, 212, 170, 0.15);
  --text: #ffffff;
  --text-muted: rgba(255, 255, 255, 0.7);
  --radius: 12px;
  --safe-bottom: env(safe-area-inset-bottom, 0px);
}

* {
  box-sizing: border-box;
}

body {
  margin: 0;
  background: var(--bg-dark);
  color: var(--text);
  font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
  min-height: 100dvh;
  padding-bottom: calc(72px + var(--safe-bottom));
}

.app {
  min-height: 100dvh;
  max-width: 480px;
  margin: 0 auto;
}

.main {
  min-height: calc(100dvh - 72px - var(--safe-bottom));
  padding: 12px;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.15s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
