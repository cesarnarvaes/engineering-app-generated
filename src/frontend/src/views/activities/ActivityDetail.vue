<template>
  <div class="activity-detail" v-loading="loading">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center">
        <el-button @click="goBack" text class="mr-4">
          <el-icon><ArrowLeft /></el-icon>
          Back
        </el-button>
        <div>
          <h1 class="text-2xl font-semibold text-gray-900">
            {{ activityData?.title }}
          </h1>
          <div class="flex items-center mt-1 space-x-4">
            <el-tag :type="getTypeTagType(activityData?.type)" size="small">
              <el-icon class="mr-1">
                <component :is="getTypeIcon(activityData?.type)" />
              </el-icon>
              {{ activityData?.type }}
            </el-tag>
            <el-tag :type="getStatusTagType(activityData?.status)" size="small">
              {{ activityData?.status }}
            </el-tag>
            <el-tag :type="getPriorityTagType(activityData?.priority)" size="small">
              {{ activityData?.priority }} priority
            </el-tag>
          </div>
        </div>
      </div>
      
      <div class="flex items-center space-x-3">
        <!-- Quick Actions based on type -->
        <el-button 
          v-if="activityData?.type === 'call' && activityData?.contact?.phone"
          @click="makeCall"
        >
          <el-icon><Phone /></el-icon>
          Call Now
        </el-button>
        
        <el-button 
          v-if="activityData?.type === 'email' && activityData?.contact?.email"
          @click="sendEmail"
        >
          <el-icon><Message /></el-icon>
          Send Email
        </el-button>
        
        <el-button 
          v-if="activityData?.type === 'meeting'"
          @click="joinMeeting"
        >
          <el-icon><VideoCamera /></el-icon>
          Join Meeting
        </el-button>
        
        <!-- Status Actions -->
        <el-button 
          v-if="activityData?.status === 'pending'" 
          type="success"
          @click="markComplete"
        >
          <el-icon><Check /></el-icon>
          Mark Complete
        </el-button>
        
        <el-button 
          v-if="activityData?.status === 'completed'" 
          type="warning"
          @click="markIncomplete"
        >
          <el-icon><RefreshLeft /></el-icon>
          Mark Incomplete
        </el-button>
        
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
            @click="saveActivity"
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
              <el-dropdown-item @click="duplicateActivity">
                <el-icon><Copy /></el-icon>
                Duplicate
              </el-dropdown-item>
              <el-dropdown-item @click="rescheduleActivity">
                <el-icon><Calendar /></el-icon>
                Reschedule
              </el-dropdown-item>
              <el-dropdown-item @click="convertActivity">
                <el-icon><Switch /></el-icon>
                Convert Type
              </el-dropdown-item>
              <el-dropdown-item divided @click="deleteActivity" class="text-red-600">
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
      <!-- Left Column - Activity Information -->
      <el-col :xs="24" :lg="16">
        <!-- Activity Overview -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <el-icon :class="getTypeIconClass(activityData?.type)" class="text-2xl mr-2">
                  <component :is="getTypeIcon(activityData?.type)" />
                </el-icon>
                <span class="text-lg font-semibold">Activity Details</span>
              </div>
              <div v-if="activityData?.dueDate" class="text-right">
                <p :class="getDueDateClass(activityData.dueDate)" class="font-semibold">
                  {{ formatDate(activityData.dueDate) }}
                </p>
                <p class="text-sm text-gray-500">
                  {{ formatTime(activityData.dueDate) }}
                </p>
              </div>
            </div>
          </template>
          
          <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <!-- Basic Information -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Title</label>
              <el-input 
                v-if="editMode"
                v-model="activityForm.title" 
                placeholder="Enter activity title"
              />
              <p v-else class="text-gray-900">{{ activityData?.title || '-' }}</p>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Type</label>
              <el-select
                v-if="editMode"
                v-model="activityForm.type"
                placeholder="Select type"
                class="w-full"
              >
                <el-option label="Call" value="call" />
                <el-option label="Email" value="email" />
                <el-option label="Meeting" value="meeting" />
                <el-option label="Task" value="task" />
              </el-select>
              <div v-else class="flex items-center">
                <el-icon :class="getTypeIconClass(activityData?.type)" class="mr-2">
                  <component :is="getTypeIcon(activityData?.type)" />
                </el-icon>
                <span class="capitalize">{{ activityData?.type || '-' }}</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Status</label>
              <el-select
                v-if="editMode"
                v-model="activityForm.status"
                placeholder="Select status"
                class="w-full"
              >
                <el-option label="Pending" value="pending" />
                <el-option label="In Progress" value="in-progress" />
                <el-option label="Completed" value="completed" />
                <el-option label="Cancelled" value="cancelled" />
              </el-select>
              <el-tag v-else :type="getStatusTagType(activityData?.status)" size="large">
                {{ activityData?.status || '-' }}
              </el-tag>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Priority</label>
              <el-select
                v-if="editMode"
                v-model="activityForm.priority"
                placeholder="Select priority"
                class="w-full"
              >
                <el-option label="Low" value="low" />
                <el-option label="Medium" value="medium" />
                <el-option label="High" value="high" />
              </el-select>
              <el-tag v-else :type="getPriorityTagType(activityData?.priority)">
                {{ activityData?.priority }} priority
              </el-tag>
            </div>

            <!-- Related Records -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Opportunity</label>
              <el-select
                v-if="editMode"
                v-model="activityForm.opportunityId"
                placeholder="Select opportunity"
                filterable
                remote
                clearable
                class="w-full"
                :remote-method="searchOpportunities"
                :loading="opportunitiesLoading"
              >
                <el-option
                  v-for="opportunity in opportunities"
                  :key="opportunity.id"
                  :label="opportunity.title"
                  :value="opportunity.id"
                />
              </el-select>
              <div v-else>
                <el-link v-if="activityData?.opportunity" @click="viewOpportunity(activityData.opportunity.id)" type="primary">
                  {{ activityData.opportunity.title }}
                </el-link>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Contact</label>
              <el-select
                v-if="editMode"
                v-model="activityForm.contactId"
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
                  :label="`${contact.firstName} ${contact.lastName}`"
                  :value="contact.id"
                />
              </el-select>
              <div v-else>
                <el-link v-if="activityData?.contact" @click="viewContact(activityData.contact.id)" type="primary">
                  {{ activityData.contact.firstName }} {{ activityData.contact.lastName }}
                </el-link>
                <p v-if="activityData?.contact?.jobTitle" class="text-sm text-gray-500">
                  {{ activityData.contact.jobTitle }}
                </p>
                <span v-if="!activityData?.contact" class="text-gray-400">-</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Company</label>
              <el-select
                v-if="editMode"
                v-model="activityForm.companyId"
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
                <el-link v-if="activityData?.company" @click="viewCompany(activityData.company.id)" type="primary">
                  {{ activityData.company.name }}
                </el-link>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Assigned To</label>
              <el-select
                v-if="editMode"
                v-model="activityForm.assignedToId"
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
                <div v-if="activityData?.assignedTo" class="flex items-center">
                  <el-avatar :size="24" class="mr-2">
                    {{ activityData.assignedTo.name.charAt(0) }}
                  </el-avatar>
                  <span>{{ activityData.assignedTo.name }}</span>
                </div>
                <span v-else class="text-gray-400">-</span>
              </div>
            </div>

            <!-- Dates -->
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Due Date</label>
              <el-date-picker
                v-if="editMode"
                v-model="activityForm.dueDate"
                type="datetime"
                placeholder="Select date and time"
                class="w-full"
              />
              <div v-else>
                <p v-if="activityData?.dueDate" :class="getDueDateClass(activityData.dueDate)">
                  {{ formatDateTime(activityData.dueDate) }}
                </p>
                <span v-else class="text-gray-400">No due date</span>
              </div>
            </div>

            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Completed Date</label>
              <el-date-picker
                v-if="editMode && activityForm.status === 'completed'"
                v-model="activityForm.completedDate"
                type="datetime"
                placeholder="Select completion date"
                class="w-full"
              />
              <div v-else>
                <p v-if="activityData?.completedDate" class="text-gray-900">
                  {{ formatDateTime(activityData.completedDate) }}
                </p>
                <span v-else class="text-gray-400">Not completed</span>
              </div>
            </div>

            <!-- Description and Notes -->
            <div class="lg:col-span-2">
              <label class="block text-sm font-medium text-gray-700 mb-1">Description</label>
              <el-input 
                v-if="editMode"
                v-model="activityForm.description" 
                type="textarea"
                :rows="3"
                placeholder="Enter activity description"
              />
              <p v-else class="text-gray-900">{{ activityData?.description || '-' }}</p>
            </div>

            <div class="lg:col-span-2">
              <label class="block text-sm font-medium text-gray-700 mb-1">Notes</label>
              <el-input 
                v-if="editMode"
                v-model="activityForm.notes" 
                type="textarea"
                :rows="3"
                placeholder="Add notes about this activity"
              />
              <p v-else class="text-gray-900">{{ activityData?.notes || '-' }}</p>
            </div>

            <!-- Contact Information (if call or email) -->
            <div v-if="(['call', 'email'].includes(activityData?.type))" class="lg:col-span-2">
              <h4 class="text-md font-medium text-gray-900 mb-3">Contact Information</h4>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div v-if="activityData?.contact?.phone">
                  <label class="block text-sm font-medium text-gray-700 mb-1">Phone</label>
                  <div class="flex items-center">
                    <span class="text-gray-900 mr-2">{{ activityData.contact.phone }}</span>
                    <el-button @click="callPhone(activityData.contact.phone)" size="small" text type="primary">
                      <el-icon><Phone /></el-icon>
                    </el-button>
                  </div>
                </div>
                
                <div v-if="activityData?.contact?.email">
                  <label class="block text-sm font-medium text-gray-700 mb-1">Email</label>
                  <div class="flex items-center">
                    <span class="text-gray-900 mr-2">{{ activityData.contact.email }}</span>
                    <el-button @click="sendEmail" size="small" text type="primary">
                      <el-icon><Message /></el-icon>
                    </el-button>
                  </div>
                </div>
              </div>
            </div>

            <!-- Meeting Information -->
            <div v-if="activityData?.type === 'meeting'" class="lg:col-span-2">
              <h4 class="text-md font-medium text-gray-900 mb-3">Meeting Information</h4>
              <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Meeting Link</label>
                  <el-input 
                    v-if="editMode"
                    v-model="activityForm.meetingLink" 
                    placeholder="Enter meeting URL"
                  />
                  <div v-else-if="activityData?.meetingLink" class="flex items-center">
                    <el-link :href="activityData.meetingLink" target="_blank" type="primary">
                      {{ activityData.meetingLink }}
                    </el-link>
                  </div>
                  <span v-else class="text-gray-400">-</span>
                </div>
                
                <div>
                  <label class="block text-sm font-medium text-gray-700 mb-1">Location</label>
                  <el-input 
                    v-if="editMode"
                    v-model="activityForm.location" 
                    placeholder="Enter location"
                  />
                  <p v-else class="text-gray-900">{{ activityData?.location || '-' }}</p>
                </div>
              </div>
            </div>
          </div>
        </el-card>

        <!-- Activity Result/Outcome -->
        <el-card v-if="activityData?.status === 'completed'" class="mb-6" shadow="hover">
          <template #header>
            <div class="flex items-center">
              <el-icon class="mr-2 text-green-600">
                <CircleCheckFilled />
              </el-icon>
              <span class="text-lg font-semibold">Activity Outcome</span>
            </div>
          </template>
          
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Result</label>
              <el-input 
                v-if="editMode"
                v-model="activityForm.result" 
                type="textarea"
                :rows="3"
                placeholder="Describe the activity outcome"
              />
              <p v-else class="text-gray-900">{{ activityData?.result || 'No outcome recorded' }}</p>
            </div>
            
            <div>
              <label class="block text-sm font-medium text-gray-700 mb-1">Next Steps</label>
              <el-input 
                v-if="editMode"
                v-model="activityForm.nextSteps" 
                type="textarea"
                :rows="2"
                placeholder="What are the next steps?"
              />
              <p v-else class="text-gray-900">{{ activityData?.nextSteps || 'No next steps defined' }}</p>
            </div>
          </div>
        </el-card>

        <!-- Follow-up Activities -->
        <el-card shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <div class="flex items-center">
                <el-icon class="mr-2 text-blue-600">
                  <Connection />
                </el-icon>
                <span class="text-lg font-semibold">Related Activities</span>
              </div>
              <el-button @click="createFollowUp" size="small" type="primary">
                <el-icon><Plus /></el-icon>
                Add Follow-up
              </el-button>
            </div>
          </template>
          
          <div v-if="relatedActivities.length > 0" class="space-y-3">
            <div 
              v-for="activity in relatedActivities"
              :key="activity.id"
              class="flex items-center justify-between p-3 rounded-lg border hover:bg-gray-50 cursor-pointer"
              @click="viewRelatedActivity(activity.id)"
            >
              <div class="flex items-center flex-1">
                <el-icon :class="getTypeIconClass(activity.type)" class="text-lg mr-3">
                  <component :is="getTypeIcon(activity.type)" />
                </el-icon>
                <div class="flex-1">
                  <h4 class="text-sm font-medium text-gray-900">{{ activity.title }}</h4>
                  <p class="text-xs text-gray-500">{{ formatDate(activity.dueDate) }}</p>
                </div>
              </div>
              <el-tag :type="getStatusTagType(activity.status)" size="small">
                {{ activity.status }}
              </el-tag>
            </div>
          </div>
          
          <div v-else class="text-center py-8 text-gray-500">
            <el-icon class="text-4xl mb-2"><Connection /></el-icon>
            <p>No related activities</p>
          </div>
        </el-card>
      </el-col>

      <!-- Right Column - Sidebar -->
      <el-col :xs="24" :lg="8">
        <!-- Timeline -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Activity Timeline</span>
          </template>
          
          <el-timeline>
            <el-timeline-item
              v-for="event in timeline"
              :key="event.id"
              :timestamp="formatDateTime(event.createdAt)"
              :type="getTimelineType(event.type)"
              size="small"
            >
              <div>
                <h4 class="text-sm font-medium text-gray-900">{{ event.title }}</h4>
                <p class="text-xs text-gray-600">{{ event.description }}</p>
                <p class="text-xs text-gray-500 mt-1">by {{ event.createdBy?.name }}</p>
              </div>
            </el-timeline-item>
          </el-timeline>
        </el-card>

        <!-- Quick Properties -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Properties</span>
          </template>
          
          <div class="space-y-3">
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Created</span>
              <span class="font-medium">{{ formatDate(activityData?.createdAt) }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Last Updated</span>
              <span class="font-medium">{{ formatDate(activityData?.updatedAt) }}</span>
            </div>
            <div class="flex justify-between items-center">
              <span class="text-gray-600">Duration</span>
              <span class="font-medium">{{ getDuration() }}</span>
            </div>
            <div v-if="activityData?.dueDate" class="flex justify-between items-center">
              <span class="text-gray-600">Time Until Due</span>
              <span :class="getDueDateClass(activityData.dueDate)" class="font-medium">
                {{ getTimeUntilDue() }}
              </span>
            </div>
          </div>
        </el-card>

        <!-- Quick Actions -->
        <el-card class="mb-6" shadow="hover">
          <template #header>
            <span class="text-lg font-semibold">Quick Actions</span>
          </template>
          
          <div class="space-y-2">
            <el-button class="w-full justify-start" @click="createFollowUp">
              <el-icon class="mr-2"><Plus /></el-icon>
              Create Follow-up
            </el-button>
            <el-button class="w-full justify-start" @click="scheduleReminder">
              <el-icon class="mr-2"><Bell /></el-icon>
              Set Reminder
            </el-button>
            <el-button class="w-full justify-start" @click="attachFile">
              <el-icon class="mr-2"><Paperclip /></el-icon>
              Attach File
            </el-button>
            <el-button class="w-full justify-start" @click="shareActivity">
              <el-icon class="mr-2"><Share /></el-icon>
              Share Activity
            </el-button>
          </div>
        </el-card>

        <!-- Attachments -->
        <el-card shadow="hover">
          <template #header>
            <div class="flex items-center justify-between">
              <span class="text-lg font-semibold">Attachments</span>
              <el-button @click="attachFile" size="small">
                <el-icon><Plus /></el-icon>
              </el-button>
            </div>
          </template>
          
          <div v-if="attachments.length > 0" class="space-y-2">
            <div 
              v-for="attachment in attachments"
              :key="attachment.id"
              class="flex items-center justify-between p-2 rounded hover:bg-gray-50"
            >
              <div class="flex items-center flex-1">
                <el-icon class="text-gray-400 mr-2">
                  <Document />
                </el-icon>
                <div class="flex-1">
                  <p class="text-sm font-medium text-gray-900 truncate">{{ attachment.name }}</p>
                  <p class="text-xs text-gray-500">{{ attachment.size }} • {{ formatDate(attachment.createdAt) }}</p>
                </div>
              </div>
              <el-button @click="downloadAttachment(attachment.id)" text size="small">
                <el-icon><Download /></el-icon>
              </el-button>
            </div>
          </div>
          
          <div v-else class="text-center py-4 text-gray-500">
            <el-icon class="text-2xl mb-1"><Document /></el-icon>
            <p class="text-sm">No attachments</p>
          </div>
        </el-card>
      </el-col>
    </el-row>

    <!-- Convert Type Dialog -->
    <el-dialog
      v-model="showConvertDialog"
      title="Convert Activity Type"
      width="30%"
    >
      <el-form>
        <el-form-item label="Current Type">
          <el-tag :type="getTypeTagType(activityData?.type)">
            {{ activityData?.type }}
          </el-tag>
        </el-form-item>
        <el-form-item label="New Type">
          <el-select v-model="newActivityType" placeholder="Select new type" class="w-full">
            <el-option label="Call" value="call" />
            <el-option label="Email" value="email" />
            <el-option label="Meeting" value="meeting" />
            <el-option label="Task" value="task" />
          </el-select>
        </el-form-item>
      </el-form>
      
      <template #footer>
        <span class="dialog-footer">
          <el-button @click="showConvertDialog = false">Cancel</el-button>
          <el-button type="primary" @click="convertActivityType">Convert</el-button>
        </span>
      </template>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { ElMessage, ElMessageBox } from 'element-plus'
import { format, differenceInDays, differenceInHours, differenceInMinutes, isPast } from 'date-fns'
import {
  ArrowLeft,
  Phone,
  Message,
  VideoCamera,
  Check,
  RefreshLeft,
  Edit,
  MoreFilled,
  Copy,
  Calendar,
  Switch,
  Delete,
  CircleCheckFilled,
  Connection,
  Plus,
  Bell,
  Paperclip,
  Share,
  Document,
  Download,
  User
} from '@element-plus/icons-vue'

// Router
const route = useRoute()
const router = useRouter()

// Reactive state
const loading = ref(true)
const saving = ref(false)
const editMode = ref(false)
const showConvertDialog = ref(false)
const opportunitiesLoading = ref(false)
const contactsLoading = ref(false)
const companiesLoading = ref(false)
const usersLoading = ref(false)

const activityData = ref(null)
const originalData = ref(null)
const opportunities = ref([])
const contacts = ref([])
const companies = ref([])
const users = ref([])
const relatedActivities = ref([])
const timeline = ref([])
const attachments = ref([])
const newActivityType = ref('')

// Form data
const activityForm = reactive({
  title: '',
  type: '',
  status: '',
  priority: '',
  opportunityId: null,
  contactId: null,
  companyId: null,
  assignedToId: null,
  dueDate: null,
  completedDate: null,
  description: '',
  notes: '',
  result: '',
  nextSteps: '',
  meetingLink: '',
  location: ''
})

// Computed
const activityId = computed(() => route.params.id)

// Methods
const loadActivity = async () => {
  try {
    loading.value = true
    
    // API call would go here
    // const response = await activitiesApi.getActivity(activityId.value)
    
    // Mock data
    const mockActivity = {
      id: activityId.value,
      title: 'Follow up call with John Smith',
      type: 'call',
      status: 'pending',
      priority: 'high',
      dueDate: '2024-03-25T10:00:00Z',
      completedDate: null,
      description: 'Discuss project timeline and next steps',
      notes: 'Customer is very interested in the proposal',
      result: '',
      nextSteps: '',
      meetingLink: '',
      location: '',
      opportunity: { id: 1, title: 'Enterprise Platform Upgrade' },
      contact: { 
        id: 1, 
        firstName: 'John', 
        lastName: 'Smith', 
        jobTitle: 'CTO',
        email: 'john.smith@acme.com',
        phone: '+1 (555) 123-4567'
      },
      company: { id: 1, name: 'Acme Corp' },
      assignedTo: { id: 1, name: 'Sarah Johnson' },
      createdAt: '2024-03-20T09:00:00Z',
      updatedAt: '2024-03-23T14:30:00Z'
    }
    
    activityData.value = mockActivity
    originalData.value = { ...mockActivity }
    
    // Populate form
    Object.assign(activityForm, {
      title: mockActivity.title,
      type: mockActivity.type,
      status: mockActivity.status,
      priority: mockActivity.priority,
      opportunityId: mockActivity.opportunity?.id,
      contactId: mockActivity.contact?.id,
      companyId: mockActivity.company?.id,
      assignedToId: mockActivity.assignedTo?.id,
      dueDate: mockActivity.dueDate ? new Date(mockActivity.dueDate) : null,
      completedDate: mockActivity.completedDate ? new Date(mockActivity.completedDate) : null,
      description: mockActivity.description,
      notes: mockActivity.notes,
      result: mockActivity.result,
      nextSteps: mockActivity.nextSteps,
      meetingLink: mockActivity.meetingLink,
      location: mockActivity.location
    })
    
    // Load related data
    await Promise.all([
      loadRelatedActivities(),
      loadTimeline(),
      loadAttachments()
    ])
    
  } catch (error) {
    console.error('Error loading activity:', error)
    ElMessage.error('Failed to load activity details')
  } finally {
    loading.value = false
  }
}

const loadRelatedActivities = async () => {
  relatedActivities.value = [
    {
      id: 2,
      title: 'Send proposal document',
      type: 'email',
      status: 'completed',
      dueDate: '2024-03-24T14:00:00Z'
    }
  ]
}

const loadTimeline = async () => {
  timeline.value = [
    {
      id: 1,
      title: 'Activity created',
      description: 'Activity created and assigned',
      type: 'create',
      createdBy: { name: 'Sarah Johnson' },
      createdAt: '2024-03-20T09:00:00Z'
    },
    {
      id: 2,
      title: 'Due date updated',
      description: 'Due date changed to March 25th',
      type: 'update',
      createdBy: { name: 'Sarah Johnson' },
      createdAt: '2024-03-22T15:30:00Z'
    }
  ]
}

const loadAttachments = async () => {
  attachments.value = [
    {
      id: 1,
      name: 'meeting-notes.pdf',
      size: '2.3 MB',
      createdAt: '2024-03-23T10:00:00Z'
    }
  ]
}

const searchOpportunities = async (query) => {
  if (!query) return
  opportunitiesLoading.value = true
  try {
    opportunities.value = [{ id: 1, title: 'Enterprise Platform Upgrade' }]
  } finally {
    opportunitiesLoading.value = false
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

const searchCompanies = async (query) => {
  if (!query) return
  companiesLoading.value = true
  try {
    companies.value = [{ id: 1, name: 'Acme Corp' }]
  } finally {
    companiesLoading.value = false
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
  Object.assign(activityForm, originalData.value)
}

const saveActivity = async () => {
  try {
    saving.value = true
    
    if (!activityForm.title) {
      ElMessage.error('Title is required')
      return
    }
    
    // API call would go here
    // const response = await activitiesApi.updateActivity(activityId.value, activityForm)
    
    Object.assign(activityData.value, activityForm)
    originalData.value = { ...activityData.value }
    
    ElMessage.success('Activity updated successfully')
    editMode.value = false
    
  } catch (error) {
    console.error('Error saving activity:', error)
    ElMessage.error('Failed to save activity')
  } finally {
    saving.value = false
  }
}

const markComplete = async () => {
  try {
    activityData.value.status = 'completed'
    activityData.value.completedDate = new Date().toISOString()
    ElMessage.success('Activity marked as completed')
  } catch (error) {
    console.error('Error marking complete:', error)
    ElMessage.error('Failed to complete activity')
  }
}

const markIncomplete = async () => {
  try {
    activityData.value.status = 'pending'
    activityData.value.completedDate = null
    ElMessage.success('Activity marked as incomplete')
  } catch (error) {
    console.error('Error marking incomplete:', error)
    ElMessage.error('Failed to mark incomplete')
  }
}

const deleteActivity = async () => {
  try {
    await ElMessageBox.confirm(
      'Are you sure you want to delete this activity? This action cannot be undone.',
      'Delete Activity',
      {
        confirmButtonText: 'Delete',
        cancelButtonText: 'Cancel',
        type: 'warning',
        confirmButtonClass: 'el-button--danger'
      }
    )
    
    ElMessage.success('Activity deleted successfully')
    router.push('/activities')
    
  } catch (error) {
    if (error !== 'cancel') {
      console.error('Error deleting activity:', error)
      ElMessage.error('Failed to delete activity')
    }
  }
}

const duplicateActivity = () => {
  router.push(`/activities/new?duplicate=${activityId.value}`)
}

const rescheduleActivity = () => {
  ElMessage.info('Reschedule functionality will be implemented')
}

const convertActivity = () => {
  newActivityType.value = ''
  showConvertDialog.value = true
}

const convertActivityType = () => {
  if (!newActivityType.value) {
    ElMessage.error('Please select a new activity type')
    return
  }
  
  activityData.value.type = newActivityType.value
  showConvertDialog.value = false
  ElMessage.success('Activity type converted successfully')
}

const goBack = () => {
  router.back()
}

const makeCall = () => {
  if (activityData.value?.contact?.phone) {
    window.open(`tel:${activityData.value.contact.phone}`)
  }
}

const sendEmail = () => {
  if (activityData.value?.contact?.email) {
    window.open(`mailto:${activityData.value.contact.email}`)
  }
}

const joinMeeting = () => {
  if (activityData.value?.meetingLink) {
    window.open(activityData.value.meetingLink, '_blank')
  } else {
    ElMessage.info('No meeting link available')
  }
}

const callPhone = (phone) => {
  window.open(`tel:${phone}`)
}

const createFollowUp = () => {
  router.push(`/activities/new?relatedToActivity=${activityId.value}`)
}

const viewRelatedActivity = (relatedActivityId) => {
  router.push(`/activities/${relatedActivityId}`)
}

const scheduleReminder = () => {
  ElMessage.info('Reminder functionality will be implemented')
}

const attachFile = () => {
  ElMessage.info('File attachment functionality will be implemented')
}

const shareActivity = () => {
  ElMessage.info('Share functionality will be implemented')
}

const downloadAttachment = (attachmentId) => {
  ElMessage.info('Download functionality will be implemented')
}

const viewOpportunity = (opportunityId) => {
  router.push(`/opportunities/${opportunityId}`)
}

const viewContact = (contactId) => {
  router.push(`/contacts/${contactId}`)
}

const viewCompany = (companyId) => {
  router.push(`/companies/${companyId}`)
}

// Helper functions
const getTypeIcon = (type) => {
  const icons = {
    call: Phone,
    email: Message,
    meeting: VideoCamera,
    task: Check
  }
  return icons[type] || Check
}

const getTypeIconClass = (type) => {
  const classes = {
    call: 'text-blue-600',
    email: 'text-green-600',
    meeting: 'text-purple-600',
    task: 'text-orange-600'
  }
  return classes[type] || 'text-gray-600'
}

const getTypeTagType = (type) => {
  const types = {
    call: 'primary',
    email: 'success',
    meeting: 'warning',
    task: 'info'
  }
  return types[type] || 'info'
}

const getStatusTagType = (status) => {
  const types = {
    pending: 'warning',
    'in-progress': 'primary',
    completed: 'success',
    cancelled: 'info'
  }
  return types[status] || 'info'
}

const getPriorityTagType = (priority) => {
  const types = {
    high: 'danger',
    medium: 'warning',
    low: 'success'
  }
  return types[priority] || 'info'
}

const getDueDateClass = (dueDate) => {
  const date = new Date(dueDate)
  const now = new Date()
  
  if (isPast(date)) {
    return 'text-red-600'
  } else if (differenceInHours(date, now) < 24) {
    return 'text-orange-600'
  } else {
    return 'text-gray-900'
  }
}

const getTimelineType = (type) => {
  const types = {
    create: 'success',
    update: 'primary',
    complete: 'success',
    delete: 'danger'
  }
  return types[type] || 'info'
}

const getDuration = () => {
  if (!activityData.value?.completedDate || !activityData.value?.createdAt) {
    return 'Ongoing'
  }
  
  const start = new Date(activityData.value.createdAt)
  const end = new Date(activityData.value.completedDate)
  const days = differenceInDays(end, start)
  
  if (days > 0) {
    return `${days} days`
  } else {
    const hours = differenceInHours(end, start)
    return `${hours} hours`
  }
}

const getTimeUntilDue = () => {
  if (!activityData.value?.dueDate) return 'No due date'
  
  const now = new Date()
  const dueDate = new Date(activityData.value.dueDate)
  
  if (isPast(dueDate)) {
    const days = differenceInDays(now, dueDate)
    return days > 0 ? `${days} days overdue` : 'Due today'
  } else {
    const days = differenceInDays(dueDate, now)
    if (days > 0) {
      return `${days} days`
    } else {
      const hours = differenceInHours(dueDate, now)
      return `${hours} hours`
    }
  }
}

const formatDate = (date) => {
  return format(new Date(date), 'MMM dd, yyyy')
}

const formatTime = (date) => {
  return format(new Date(date), 'HH:mm')
}

const formatDateTime = (date) => {
  return format(new Date(date), 'MMM dd, yyyy HH:mm')
}

// Lifecycle
onMounted(() => {
  loadActivity()
})

// Watch for route changes
watch(() => route.params.id, (newId) => {
  if (newId) {
    loadActivity()
  }
}, { immediate: true })
</script>

<style scoped>
.activity-detail {
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

@media (min-width: 640px) {
  .sm\:grid-cols-2 {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

/* Mobile responsiveness */
@media (max-width: 768px) {
  .activity-detail {
    padding: 0 1rem;
  }
  
  .activity-detail :deep(.el-card__body) {
    padding: 16px;
  }
}
</style>