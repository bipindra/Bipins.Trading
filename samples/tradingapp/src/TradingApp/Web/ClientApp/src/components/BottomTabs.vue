<template>
  <nav class="bottom-tabs">
    <router-link to="/" class="tab" :class="{ active: activeTab === 'home' }" aria-label="Home">
      <HomeIcon class="icon" />
    </router-link>
    <router-link to="/search" class="tab" :class="{ active: activeTab === 'search' }" aria-label="Search">
      <MagnifyingGlassIcon class="icon" />
    </router-link>
    <router-link to="/notifications" class="tab" :class="{ active: activeTab === 'notifications' }" aria-label="Notifications">
      <BellIcon class="icon" />
    </router-link>
    <router-link to="/monitoring" class="tab" :class="{ active: activeTab === 'monitoring' }" aria-label="Monitoring">
      <ChartBarIcon class="icon" />
    </router-link>
    <router-link to="/settings" class="tab" :class="{ active: activeTab === 'settings' }" aria-label="Settings">
      <Cog6ToothIcon class="icon" />
    </router-link>
  </nav>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { HomeIcon, MagnifyingGlassIcon, BellIcon, ChartBarIcon, Cog6ToothIcon } from '@heroicons/vue/24/outline'

const route = useRoute()
const activeTab = computed(() => route.meta?.tab ?? null)
</script>

<style scoped>
.bottom-tabs {
  position: fixed;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 100%;
  max-width: 480px;
  height: calc(56px + var(--safe-bottom));
  padding-bottom: var(--safe-bottom);
  background: var(--bg-card);
  border-top: 1px solid rgba(255, 255, 255, 0.08);
  display: flex;
  justify-content: space-around;
  align-items: center;
  z-index: 100;
}

.tab {
  display: flex;
  align-items: center;
  justify-content: center;
  min-width: 56px;
  min-height: 56px;
  color: var(--text-muted);
  text-decoration: none;
  border-radius: var(--radius);
  transition: color 0.2s, background 0.2s;
}

.tab:hover {
  color: var(--text);
}

.tab.active {
  color: var(--aqua);
}

.icon {
  width: 28px;
  height: 28px;
}
</style>
