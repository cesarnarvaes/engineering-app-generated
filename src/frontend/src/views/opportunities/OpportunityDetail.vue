<template>
  <div class="opportunity-detail" v-loading="loading">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center">
        <el-button @click="goBack" text class="mr-4">
          <el-icon><ArrowLeft /></el-icon>
          Back
        </el-button>
        <div>
          <h1 class="text-2xl font-semibold text-gray-900">
            {{ opportunityData?.title }}
          </h1>
          <div class="flex items-center mt-1 space-x-4">
            <p class="text-gray-600">{{ opportunityData?.company?.name }}</p>
            <el-tag :type="getStageTagType(opportunityData?.stage)" size="small">
              {{ opportunityData?.stage }}
            </el-tag>
            <el-tag :type="getStatusTagType(opportunityData?.status)" size="small">
              {{ opportunityData?.status }}
            </el-tag>
          </div>
        </div>
      </div>
      
      <div class="flex items-center space-x-3">
        <!-- Quick Actions -->
        <el-button @click="sendEmail" :disabled="!opportunityData?.contact?.email">
          <el-icon><Message /></el-icon>
          Email Contact
        </el-button>
        <el-button @click="scheduleCall">
          <el-icon><Phone /></el-icon>
          Schedule Call
        </el-button>
        
        <!-- Stage Actions -->
        <el-dropdown>
          <el-button type="primary">
            <el-icon><RefreshRight /></el-icon>
            Change Stage
            <el-icon><ArrowDown /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item 
                v-for="stage in availableStages" 
                :key="stage"
                @click="changeStage(stage)"
              >
                {{ stage }}
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
        
        <!-- Edit/Save Toggle -->
        <el-button 
          v-if="!editMode" 
          @click="toggleEditMode"
        >
          <el-icon><Edit /></el-icon>
          Edit
        </el-button>
        
        <div v-else class="flex space-x-2">
          <el-button @click="cancelEdit">
            Cancel
          </el-button>
          <el-button 
            type="primary" 
            :loading="saving"
            @click="saveOpportunity"
          >
            <el-icon><Check /></el-icon>
            Save
          </el-button>
        </div>
        
        <!-- More Actions -->
        <el-dropdown>
          <el-button>
            <el-icon><MoreFilled /></el-icon>
          </el-button>
          <template #dropdown>
            <el-dropdown-menu>
              <el-dropdown-item @click="duplicateOpportunity">
                <el-icon><CopyDocument /></el-icon>
                Duplicate
              </el-dropdown-item>
              <el-dropdown-item @click="generateProposal">
                <el-icon><Document /></el-icon>
                Generate Proposal
              </el-dropdown-item>
              <el-dropdown-item @click="exportOpportunity">
                <el-icon><Download /></el-icon>
                Export
              </el-dropdown-item>
              <el-dropdown-item divided @click="deleteOpportunity" class="text-red-600">
                <el-icon><Delete /></el-icon>
                Delete
              </el-dropdown-item>
            </el-dropdown-menu>
          </template>
        </el-dropdown>
      </div>
    </div>

    <!-- Main Content -->
    <el-row :gutter="20">
      <!-- Left Column - Opportunity Information -->
      <el-col :xs="24" :lg="16">
        <!-- Opportunity Overview -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <el-icon class="mr-2 text-blue-600">
                  <TrendCharts />
                </el-icon>
                <span class="text-lg font-semibold">Opportunity Details</span>
              </div>
              <div class="text-2xl font-bold text-green-600">
                ${{ formatCurrency(opportunityData?.estimatedValue || 0) }}
              </div>
            </div>
          </template>
          
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <!-- Basic Information -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Title</label>
              <el-input 
                v-if="editMode"
                v-model="opportunityForm.title" 
                placeholder="Enter opportunity title"
              />
              <p v-else class="text-gray-900">{{ opportunityData?.title || '-' }}</p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Company</label>
              <el-select
                v-if="editMode"
                v-model="opportunityForm.companyId"
                placeholder="Select company"
                filterable
                remote
                clearable
                class="w-full"
                :remote-method="searchCompanies"
                :loading="companiesLoading"
              >
                <el-option
                  v-for="company in companies"
                  :key="company.id"
                  :label="company.name"
                  :value="company.id"
                />
              </el-select>
              <div v-else>
                <el-link v-if="opportunityData?.company" @click="viewCompany(opportunityData.company.id)" type="primary">
                  {{ opportunityData.company.name }}
                </el-link>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Contact</label>
              <el-select
                v-if="editMode"
                v-model="opportunityForm.contactId"
                placeholder="Select contact"
                filterable
                remote
                clearable
                class="w-full"
                :remote-method="searchContacts"
                :loading="contactsLoading"
              >
                <el-option
                  v-for="contact in contacts"
                  :key="contact.id"
                  :label="`${contact.firstName} ${contact.lastName} - ${contact.jobTitle}`"
                  :value="contact.id"
                />
              </el-select>
              <div v-else>
                <el-link v-if="opportunityData?.contact" @click="viewContact(opportunityData.contact.id)" type="primary">
                  {{ opportunityData.contact.firstName }} {{ opportunityData.contact.lastName }}
                </el-link>
                <p v-if="opportunityData?.contact?.jobTitle" class="text-sm text-gray-500">
                  {{ opportunityData.contact.jobTitle }}
                </p>
                <span v-if="!opportunityData?.contact" class="text-gray-400">-</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Assigned To</label>
              <el-select
                v-if="editMode"
                v-model="opportunityForm.assignedToId"
                placeholder="Select user"
                filterable
                remote
                clearable
                class="w-full"
                :remote-method="searchUsers"
                :loading="usersLoading"
              >
                <el-option
                  v-for="user in users"
                  :key="user.id"
                  :label="user.name"
                  :value="user.id"
                />
              </el-select>
              <div v-else>
                <div v-if="opportunityData?.assignedTo" class="flex items-center">
                  <el-avatar :size="24" class="mr-2">
                    {{ opportunityData.assignedTo.name.charAt(0) }}
                  </el-avatar>
                  <span>{{ opportunityData.assignedTo.name }}</span>
                </div>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <!-- Financial Information -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Estimated Value</label>
              <el-input 
                v-if="editMode"
                v-model="opportunityForm.estimatedValue" 
                placeholder="Enter estimated value"
                type="number"
              >
                <template #prepend>$</template>
              </el-input>
              <div v-else class="text-2xl font-bold text-green-600">
                ${{ formatCurrency(opportunityData?.estimatedValue || 0) }}
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Probability</label>
              <div v-if="editMode" class="flex items-center space-x-3">
                <el-slider 
                  v-model="opportunityForm.probabilityPercentage" 
                  :min="0"
                  :max="100"
                  :step="5"
                  class="flex-1"
                />
                <span class="text-sm font-medium w-12">{{ opportunityForm.probabilityPercentage }}%</span>
              </div>
              <div v-else class="flex items-center">
                <el-progress 
                  :percentage="opportunityData?.probabilityPercentage || 0" 
                  :stroke-width="10"
                  class="flex-1 mr-3"
                />
                <span class="text-sm font-medium">{{ opportunityData?.probabilityPercentage }}%</span>
              </div>
            </div>

            <!-- Dates -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Expected Close Date</label>
              <el-date-picker
                v-if="editMode"
                v-model="opportunityForm.expectedCloseDate"
                type="date"
                placeholder="Select date"
                class="w-full"
              />
              <p v-else class="text-gray-900">
                {{ opportunityData?.expectedCloseDate ? formatDate(opportunityData.expectedCloseDate) : '-' }}
              </p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Actual Close Date</label>
              <el-date-picker
                v-if="editMode"
                v-model="opportunityForm.actualCloseDate"
                type="date"
                placeholder="Select date"
                class="w-full"
              />
              <p v-else class="text-gray-900">
                {{ opportunityData?.actualCloseDate ? formatDate(opportunityData.actualCloseDate) : 'Not closed' }}
              </p>
            </div>

            <!-- Description -->
            <div class="lg:col-span-2">
              <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
              <el-input 
                v-if="editMode"
                v-model="opportunityForm.description" 
                type="textarea"
                :rows="3"
                placeholder="Enter opportunity description"
              />
              <p v-else class="text-gray-900">{{ opportunityData?.description || '-' }}</p>
            </div>

            <!-- Notes -->
            <div class="lg:col-span-2">
              <label class="block text-sm font-medium text-gray-700 mb-1">Notes</label>
              <el-input 
                v-if="editMode"
                v-model="opportunityForm.notes" 
                type="textarea"
                :rows="3"
                placeholder="Add notes about this opportunity"
              />
              <p v-else class="text-gray-900">{{ opportunityData?.notes || '-' }}</p>
            </div>
          </div>
        </el-card>

        <!-- Activities -->
        <el-card shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <el-icon class="mr-2 text-green-600">
                  <Clock />
                </el-icon>
                <span class="text-lg font-semibold">Activities & Notes</span>
              </div>
              <el-button @click="addActivity" size="small" type="primary">
                <el-icon><Plus /></el-icon>
                Add Activity
              </el-button>
            </div>
          </template>
          
          <el-timeline>
            <el-timeline-item
              v-for="activity in activities"
              :key="activity.id"
              :timestamp="formatDate(activity.createdAt)"
              :type="getActivityType(activity.type)"
            >
              <el-card shadow="never" class="activity-card">
                <div class="flex items-start justify-between">
                  <div class="flex-1">
                    <h4 class="text-sm font-medium text-gray-900 mb-2">{{ activity.title }}</h4>
                    <p class="text-sm text-gray-600 mb-2">{{ activity.description }}</p>
                    <div class="flex items-center">
                      <el-tag :type="getActivityTagType(activity.type)" size="small" class="mr-2">
                        {{ activity.type }}
                      </el-tag>
                      <span class="text-xs text-gray-500">by {{ activity.createdBy?.name }}</span>
                    </div>
                  </div>
                  <el-button text size="small" @click="editActivity(activity.id)">
                    <el-icon><Edit /></el-icon>
                  </el-button>
                </div>
              </el-card>
            </el-timeline-item>
          </el-timeline>
          
          <div v-if="activities.length === 0" class="text-center py-8 text-gray-500">
            <el-icon class="text-4xl mb-2"><Calendar /></el-icon>
            <p>No activities recorded</p>
          </div>
        </el-card>
      </el-col>

      <!-- Right Column - Sidebar -->
      <el-col :xs="24" :lg="8">
        <!-- Stage Progress -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Sales Stage Progress</span>
          </template>
          
          <div class="space-y-4">
            <div 
              v-for="(stage, index) in allStages" 
              :key="stage"
              class="flex items-center"
            >
              <div 
                :class="[
                  'w-8 h-8 rounded-full flex items-center justify-center mr-3',
                  getStageIndex(opportunityData?.stage) >= index 
                    ? 'bg-green-100 text-green-600 border-2 border-green-600' 
                    : 'bg-gray-100 text-gray-400 border-2 border-gray-300'
                ]"
              >
                <el-icon v-if="getStageIndex(opportunityData?.stage) >= index">
                  <Check />
                </el-icon>
                <span v-else class="text-xs font-medium">{{ index + 1 }}</span>
              </div>
              <div class="flex-1">
                <p :class="[
                  'text-sm font-medium',
                  getStageIndex(opportunityData?.stage) >= index ? 'text-gray-900' : 'text-gray-500'
                ]">
                  {{ stage }}
                </p>
              </div>
            </div>
          </div>
        </el-card>

        <!-- Key Metrics -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Key Metrics</span>
          </template>
          
          <div class="space-y-4">
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Weighted Value</span>
              <span class="font-semibold text-green-600">
                ${{ formatCurrency(getWeightedValue()) }}
              </span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Days in Stage</span>
              <span class="font-semibold">{{ getDaysInStage() }} days</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Days to Close</span>
              <span class="font-semibold">{{ getDaysToClose() }} days</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Last Activity</span>
              <span class="font-semibold">{{ getLastActivity() }}</span>
            </div>
          </div>
        </el-card>

        <!-- Related Records -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Related Records</span>
          </template>
          
          <div class="space-y-3">
            <div class="flex items-center justify-between p-2 rounded hover:bg-gray-50">
              <div class="flex items-center">
                <el-icon class="text-blue-600 mr-2">
                  <Document />
                </el-icon>
                <span class="text-sm">Proposals</span>
              </div>
              <el-badge :value="relatedCounts.proposals" class="mr-2" />
            </div>
            
            <div class="flex items-center justify-between p-2 rounded hover:bg-gray-50">
              <div class="flex items-center">
                <el-icon class="text-green-600 mr-2">
                  <Phone />
                </el-icon>
                <span class="text-sm">Calls</span>
              </div>
              <el-badge :value="relatedCounts.calls" class="mr-2" />
            </div>
            
            <div class="flex items-center justify-between p-2 rounded hover:bg-gray-50">
              <div class="flex items-center">
                <el-icon class="text-purple-600 mr-2">
                  <Message />
                </el-icon>
                <span class="text-sm">Emails</span>
              </div>
              <el-badge :value="relatedCounts.emails" class="mr-2" />
            </div>
            
            <div class="flex items-center justify-between p-2 rounded hover:bg-gray-50">
              <div class="flex items-center">
                <el-icon class="text-orange-600 mr-2">
                  <Calendar />
                </el-icon>
                <span class="text-sm">Meetings</span>
              </div>
              <el-badge :value="relatedCounts.meetings" class="mr-2" />
            </div>
          </div>
        </el-card>

        <!-- Quick Actions -->
        <el-card shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Quick Actions</span>
          </template>
          
          <div class="space-y-2">
            <el-button class="w-full justify-start" @click="scheduleCall">
              <el-icon class="mr-2"><Phone /></el-icon>
              Schedule Call
            </el-button>
            <el-button class="w-full justify-start" @click="sendEmail">
              <el-icon class="mr-2"><Message /></el-icon>
              Send Email
            </el-button>
            <el-button class="w-full justify-start" @click="createProposal">
              <el-icon class="mr-2"><Document /></el-icon>
              Create Proposal
            </el-button>
            <el-button class="w-full justify-start" @click="addTask">
              <el-icon class="mr-2"><Plus /></el-icon>
              Add Task
            </el-button>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Stage Change Dialog -->
    <el-dialog
      v-model="showStageDialog"
      title="Change Opportunity Stage"
      width="30%"
      :close-on-click-modal="false"
    >
      <el-form>
        <el-form-item label="Current Stage">
          <el-tag :type="getStageTagType(opportunityData?.stage)">
            {{ opportunityData?.stage }}
          </el-tag>
        </el-form-item>
        <el-form-item label="New Stage">
          <el-tag :type="getStageTagType(newStage)">
            {{ newStage }}
          </el-tag>
        </el-form-item>
        <el-form-item label="Notes">
          <el-input 
            v-model="stageChangeNotes" 
            type="textarea" 
            :rows="3"
            placeholder="Add notes about this stage change..."
          />
        </el-form-item>
      </el-form>
      
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showStageDialog = false">Cancel</el-button>
          <el-button 
            type="primary" 
            :loading="changingStage"
            @click="updateOpportunityStage"
          >
            Update Stage
          </el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { format, differenceInDays } from 'date-fns'
import {
  ArrowLeft,
  Message,
  Phone,
  RefreshRight,
  ArrowDown,
  Edit,
  Check,
  MoreFilled,
  CopyDocument,
  Document,
  Download,
  Delete,
  TrendCharts,
  Clock,
  Plus,
  Calendar
} from '@element-plus/icons-vue'

// Router
const route = useRoute()
const router = useRouter()

// Reactive state
const loading = ref(true)
const saving = ref(false)
const changingStage = ref(false)
const editMode = ref(false)
const showStageDialog = ref(false)
const companiesLoading = ref(false)
const contactsLoading = ref(false)
const usersLoading = ref(false)

const opportunityData = ref(null)
const originalData = ref(null)
const companies = ref([])
const contacts = ref([])
const users = ref([])
const activities = ref([])
const newStage = ref('')
const stageChangeNotes = ref('')

const allStages = ['Prospecting', 'Qualification', 'Proposal', 'Negotiation', 'Closed Won']

// Form data
const opportunityForm = reactive({
  title: '',
  companyId: null,
  contactId: null,
  assignedToId: null,
  estimatedValue: null,
  probabilityPercentage: 50,
  expectedCloseDate: null,
  actualCloseDate: null,
  description: '',
  notes: ''
})

const relatedCounts = reactive({
  proposals: 2,
  calls: 5,
  emails: 8,
  meetings: 3
})

// Computed
const opportunityId = computed(() => route.params.id)

const availableStages = computed(() => {
  const currentStage = opportunityData.value?.stage
  return allStages.filter(stage => stage !== currentStage)
})

// Methods
const loadOpportunity = async () => {
  try {
    loading.value = true
    
    // API call would go here
    // const response = await opportunitiesApi.getOpportunity(opportunityId.value)
    
    // Mock data
    const mockOpportunity = {
      id: opportunityId.value,
      title: 'Enterprise Platform Upgrade',
      estimatedValue: 150000,
      probabilityPercentage: 75,
      stage: 'Proposal',
      status: 'Open',
      expectedCloseDate: '2024-04-30T00:00:00Z',
      actualCloseDate: null,
      description: 'Complete upgrade of enterprise platform including new features and performance improvements.',
      notes: 'Customer is very interested and has budget approved.',
      company: { id: 1, name: 'Acme Corp' },
      contact: { id: 1, firstName: 'John', lastName: 'Smith', jobTitle: 'CTO', email: 'john.smith@acme.com' },
      assignedTo: { id: 1, name: 'Sarah Johnson' },
      createdAt: '2024-01-15T10:00:00Z',
      updatedAt: '2024-03-20T14:30:00Z'
    }
    
    opportunityData.value = mockOpportunity
    originalData.value = { ...mockOpportunity }
    
    // Populate form
    Object.assign(opportunityForm, {
      title: mockOpportunity.title,
      companyId: mockOpportunity.company?.id,
      contactId: mockOpportunity.contact?.id,
      assignedToId: mockOpportunity.assignedTo?.id,
      estimatedValue: mockOpportunity.estimatedValue,
      probabilityPercentage: mockOpportunity.probabilityPercentage,
      expectedCloseDate: mockOpportunity.expectedCloseDate ? new Date(mockOpportunity.expectedCloseDate) : null,
      actualCloseDate: mockOpportunity.actualCloseDate ? new Date(mockOpportunity.actualCloseDate) : null,
      description: mockOpportunity.description,
      notes: mockOpportunity.notes
    })
    
    // Load related data
    await loadActivities()
    
  } catch (error) {
    console.error('Error loading opportunity:', error)
    ElMessage.error('Failed to load opportunity details')
  } finally {
    loading.value = false
  }
}

const loadActivities = async () => {
  // Mock activities
  activities.value = [
    {
      id: 1,
      title: 'Initial discovery call',
      description: 'Discussed project requirements and timeline',
      type: 'call',
      createdBy: { name: 'Sarah Johnson' },
      createdAt: '2024-03-15T10:00:00Z'
    },
    {
      id: 2,
      title: 'Proposal sent',
      description: 'Sent detailed proposal with pricing and timeline',
      type: 'email',
      createdBy: { name: 'Sarah Johnson' },
      createdAt: '2024-03-18T14:30:00Z'
    }
  ]
}

const searchCompanies = async (query) => {
  if (!query) return
  companiesLoading.value = true
  try {
    companies.value = [{ id: 1, name: 'Acme Corp' }]
  } finally {
    companiesLoading.value = false
  }
}

const searchContacts = async (query) => {
  if (!query) return
  contactsLoading.value = true
  try {
    contacts.value = [
      { id: 1, firstName: 'John', lastName: 'Smith', jobTitle: 'CTO' }
    ]
  } finally {
    contactsLoading.value = false
  }
}

const searchUsers = async (query) => {
  if (!query) return
  usersLoading.value = true
  try {
    users.value = [{ id: 1, name: 'Sarah Johnson' }]
  } finally {
    usersLoading.value = false
  }
}

const toggleEditMode = () => {
  editMode.value = !editMode.value
}

const cancelEdit = () => {
  editMode.value = false
  Object.assign(opportunityForm, originalData.value)
}

const saveOpportunity = async () => {
  try {
    saving.value = true
    
    if (!opportunityForm.title) {
      ElMessage.error('Title is required')
      return
    }
    
    // API call would go here
    // const response = await opportunitiesApi.updateOpportunity(opportunityId.value, opportunityForm)
    
    Object.assign(opportunityData.value, opportunityForm)
    originalData.value = { ...opportunityData.value }
    
    ElMessage.success('Opportunity updated successfully')
    editMode.value = false
    
  } catch (error) {
    console.error('Error saving opportunity:', error)
    ElMessage.error('Failed to save opportunity')
  } finally {
    saving.value = false
  }
}

const changeStage = (stage) => {
  newStage.value = stage
  stageChangeNotes.value = ''
  showStageDialog.value = true
}

const updateOpportunityStage = async () => {
  try {
    changingStage.value = true
    
    // API call would go here
    // await opportunitiesApi.updateStage(opportunityId.value, { stage: newStage.value, notes: stageChangeNotes.value })
    
    opportunityData.value.stage = newStage.value
    
    ElMessage.success('Opportunity stage updated successfully')
    showStageDialog.value = false
    
  } catch (error) {
    console.error('Error updating stage:', error)
    ElMessage.error('Failed to update stage')
  } finally {
    changingStage.value = false
  }
}

const deleteOpportunity = async () => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this opportunity? This action cannot be undone.',
      'Delete Opportunity',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
        confirmButtonClass: 'el-button--danger'
      }
    )
    
    ElMessage.success('Opportunity deleted successfully')
    router.push('/opportunities')
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting opportunity:', error)
      ElMessage.error('Failed to delete opportunity')
    }
  }
}

const duplicateOpportunity = () => {
  router.push(`/opportunities/new?duplicate=${opportunityId.value}`)
}

const generateProposal = () => {
  ElMessage.info('Proposal generation will be implemented')
}

const exportOpportunity = () => {
  ElMessage.info('Export functionality will be implemented')
}

const goBack = () => {
  router.back()
}

const sendEmail = () => {
  if (opportunityData.value?.contact?.email) {
    window.open(`mailto:${opportunityData.value.contact.email}`)
  }
}

const scheduleCall = () => {
  router.push(`/activities/new?opportunityId=${opportunityId.value}&type=call`)
}

const addActivity = () => {
  router.push(`/activities/new?opportunityId=${opportunityId.value}`)
}

const addTask = () => {
  router.push(`/activities/new?opportunityId=${opportunityId.value}&type=task`)
}

const createProposal = () => {
  ElMessage.info('Proposal creation will be implemented')
}

const editActivity = (activityId) => {
  router.push(`/activities/${activityId}/edit`)
}

const viewCompany = (companyId) => {
  router.push(`/companies/${companyId}`)
}

const viewContact = (contactId) => {
  router.push(`/contacts/${contactId}`)
}

// Helper functions
const getStageTagType = (stage) => {
  const types = {
    'Prospecting': 'info',
    'Qualification': 'primary',
    'Proposal': 'warning',
    'Negotiation': 'danger',
    'Closed Won': 'success',
    'Closed Lost': 'info'
  }
  return types[stage] || 'info'
}

const getStatusTagType = (status) => {
  const types = {
    'Open': 'success',
    'Closed': 'info'
  }
  return types[status] || 'info'
}

const getActivityType = (type) => {
  const types = {
    call: 'primary',
    email: 'success',
    meeting: 'warning',
    task: 'info'
  }
  return types[type] || 'info'
}

const getActivityTagType = (type) => {
  const types = {
    call: 'primary',
    email: 'success',
    meeting: 'warning',
    task: 'info'
  }
  return types[type] || 'info'
}

const getStageIndex = (stage) => {
  return allStages.indexOf(stage)
}

const getWeightedValue = () => {
  const value = opportunityData.value?.estimatedValue || 0
  const probability = opportunityData.value?.probabilityPercentage || 0
  return (value * probability) / 100
}

const getDaysInStage = () => {
  // Mock calculation - would be based on last stage change date
  return 12
}

const getDaysToClose = () => {
  if (!opportunityData.value?.expectedCloseDate) return 'TBD'
  return differenceInDays(new Date(opportunityData.value.expectedCloseDate), new Date())
}

const getLastActivity = () => {
  if (activities.value.length === 0) return 'Never'
  const lastActivity = activities.value[activities.value.length - 1]
  return format(new Date(lastActivity.createdAt), 'MMM dd')
}

const formatCurrency = (amount) => {
  return new Intl.NumberFormat('en-US', {
    notation: 'compact',
    compactDisplay: 'short'
  }).format(amount)
}

const formatDate = (date) => {
  return format(new Date(date), 'MMM dd, yyyy')
}

// Lifecycle
onMounted(() => {
  loadOpportunity()
})

// Watch for route changes
watch(() => route.params.id, (newId) => {
  if (newId) {
    loadOpportunity()
  }
}, { immediate: true })
</script>

<style scoped>
.opportunity-detail {
  min-height: calc(100vh - 200px);
}

.grid {
  display: grid;
  gap: 1.5rem;
}

.grid-cols-1 {
  grid-template-columns: repeat(1, minmax(0, 1fr));
}

@media (min-width: 1024px) {
  .lg\:grid-cols-2 {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
  
  .lg\:col-span-2 {
    grid-column: span 2;
  }
}

.activity-card {
  background-color: #f9fafb;
  border: 1px solid #e5e7eb;
}

.activity-card :deep(.el-card__body) {
  padding: 1rem;
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .opportunity-detail {
    padding: 0 1rem;
  }
  
  .opportunity-detail :deep(.el-card__body) {
    padding: 16px;
  }
}
</style>