<template>
  <div class="chart-container">
    <svg :viewBox="`0 0 ${width} ${height}`" class="chart-svg">
      <defs>
        <linearGradient id="lineGradient" x1="0%" y1="0%" x2="0%" y2="100%">
          <stop offset="0%" style="stop-color:#00D9FF;stop-opacity:0.3" />
          <stop offset="100%" style="stop-color:#00D9FF;stop-opacity:0" />
        </linearGradient>
      </defs>
      <g :transform="`translate(${padding}, ${padding})`">
        <!-- Area fill -->
        <path
          v-if="areaPath"
          :d="areaPath"
          fill="url(#lineGradient)"
          class="area"
        />
        <!-- Line -->
        <path
          v-if="linePath"
          :d="linePath"
          fill="none"
          stroke="#00D9FF"
          stroke-width="2"
          class="line"
        />
      </g>
    </svg>
    <div v-if="!hasData" class="no-data">No data available</div>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  data: { type: Array, default: () => [] },
  width: { type: Number, default: 400 },
  height: { type: Number, default: 120 },
  padding: { type: Number, default: 10 }
})

const hasData = computed(() => props.data && props.data.length > 0)

const chartWidth = computed(() => props.width - props.padding * 2)
const chartHeight = computed(() => props.height - props.padding * 2)

const values = computed(() => {
  if (!hasData.value) return []
  return props.data.map(d => typeof d === 'number' ? d : (d.close || d.value || 0))
})

const minValue = computed(() => {
  if (values.value.length === 0) return 0
  return Math.min(...values.value)
})

const maxValue = computed(() => {
  if (values.value.length === 0) return 0
  return Math.max(...values.value)
})

const range = computed(() => {
  const diff = maxValue.value - minValue.value
  return diff === 0 ? 1 : diff
})

const points = computed(() => {
  if (!hasData.value) return []
  return values.value.map((value, index) => {
    const x = (index / (values.value.length - 1 || 1)) * chartWidth.value
    const y = chartHeight.value - ((value - minValue.value) / range.value) * chartHeight.value
    return { x, y }
  })
})

const linePath = computed(() => {
  if (points.value.length === 0) return null
  if (points.value.length === 1) {
    const p = points.value[0]
    return `M ${p.x} ${p.y} L ${p.x} ${p.y}`
  }
  return points.value
    .map((p, i) => (i === 0 ? `M ${p.x} ${p.y}` : `L ${p.x} ${p.y}`))
    .join(' ')
})

const areaPath = computed(() => {
  if (points.value.length === 0) return null
  const first = points.value[0]
  const last = points.value[points.value.length - 1]
  return `${linePath.value} L ${last.x} ${chartHeight.value} L ${first.x} ${chartHeight.value} Z`
})
</script>

<style scoped>
.chart-container {
  position: relative;
  width: 100%;
  height: 100%;
  min-height: 120px;
}

.chart-svg {
  width: 100%;
  height: 100%;
  display: block;
}

.line {
  stroke-linecap: round;
  stroke-linejoin: round;
}

.area {
  opacity: 0.3;
}

.no-data {
  position: absolute;
  inset: 0;
  display: flex;
  align-items: center;
  justify-content: center;
  color: var(--text-muted);
  font-size: 14px;
}
</style>
