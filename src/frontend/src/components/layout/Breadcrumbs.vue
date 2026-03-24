<template>
  <el-breadcrumb separator="/" class="mb-6">
    <el-breadcrumb-item 
      v-for="(item, index) in breadcrumbs" 
      :key="index"
      :to="item.to"
    >
      <el-icon v-if="item.icon" class="mr-1">
        <component :is="item.icon" />
      </el-icon>
      {{ item.text }}
    </el-breadcrumb-item>
  </el-breadcrumb>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'

// Props
const props = defineProps({
  items: {
    type: Array,
    default: () => []
  }
})

const route = useRoute()

// Computed
const breadcrumbs = computed(() => {
  // If items are passed as props, use them
  if (props.items.length > 0) {
    return props.items
  }
  
  // Otherwise, generate breadcrumbs from route
  const segments = route.path.split('/').filter(Boolean)
  const breadcrumbItems = [{ text: 'Home', to: '/dashboard' }]
  
  let currentPath = ''
  
  segments.forEach((segment, index) => {
    currentPath += `/${segment}`
    
    // Skip numeric segments (likely IDs)
    if (!/^\d+$/.test(segment)) {
      const isLast = index === segments.length - 1
      const item = {
        text: formatSegment(segment),
        to: isLast ? undefined : currentPath
      }
      
      breadcrumbItems.push(item)
    }
  })
  
  return breadcrumbItems
})

// Methods
const formatSegment = (segment) => {
  // Handle special cases
  const specialCases = {
    'contacts': 'Contacts',
    'companies': 'Companies',
    'opportunities': 'Opportunities',
    'activities': 'Activities',
    'reports': 'Reports',
    'admin': 'Administration',
    'profile': 'Profile',
    'settings': 'Settings',
    'new': 'New',
    'edit': 'Edit'
  }
  
  if (specialCases[segment]) {
    return specialCases[segment]
  }
  
  // Capitalize first letter and replace hyphens/underscores with spaces
  return segment
    .replace(/[-_]/g, ' ')
    .replace(/\b\w/g, (char) => char.toUpperCase())
}
</script>

<style scoped>
:deep(.el-breadcrumb__item) {
  font-size: 14px;
}

:deep(.el-breadcrumb__item:not(:last-child) .el-breadcrumb__inner) {
  color: #6b7280;
  font-weight: normal;
}

:deep(.el-breadcrumb__item:not(:last-child) .el-breadcrumb__inner:hover) {
  color: #1d4ed8;
}

:deep(.el-breadcrumb__item:last-child .el-breadcrumb__inner) {
  color: #374151;
  font-weight: 500;
}

:deep(.el-breadcrumb__separator) {
  color: #d1d5db;
  margin: 0 8px;
}
</style>