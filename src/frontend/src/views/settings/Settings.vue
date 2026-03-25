<template>
  <div class="p-6">
    <div class="mb-6">
      <h1 class="text-2xl font-semibold text-gray-900">Settings</h1>
      <p class="mt-2 text-sm text-gray-700">
        Manage your application settings and preferences
      </p>
    </div>
    
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <!-- General Settings -->
      <div class="bg-white shadow-sm rounded-lg">
        <div class="p-6">
          <h2 class="text-lg font-medium text-gray-900 mb-4">General Settings</h2>
          <p class="text-gray-500">
            General application settings coming soon...
          </p>
        </div>
      </div>
      
      <!-- Notification Settings -->
      <div class="bg-white shadow-sm rounded-lg">
        <div class="p-6">
          <h2 class="text-lg font-medium text-gray-900 mb-4">Notifications</h2>
          <p class="text-gray-500">
            Notification preferences coming soon...
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
export default {
  name: 'Settings',
  meta: {
    title: 'Settings'
  }
}
</script>