import { defineStore } from 'pinia'

export const useToastStore = defineStore('toast', {
  state: () => ({
    message: '',
    visible: false,
    type: 'info' // info | success | error
  }),
  actions: {
    show(msg, type = 'info') {
      this.message = msg
      this.type = type
      this.visible = true
      setTimeout(() => {
        this.visible = false
      }, 3000)
    },
    success(msg) {
      this.show(msg, 'success')
    },
    error(msg) {
      this.show(msg, 'error')
    }
  }
})
