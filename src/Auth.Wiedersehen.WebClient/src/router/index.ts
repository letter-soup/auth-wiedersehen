import { createRouter, createWebHistory } from 'vue-router'
import SignInView from '@/views/sign-in/SignInView.vue'
import SignUpView from '@/views/sign-up/SignUpView.vue'
import ResetPasswordView from '@/views/reset-password/ResetPasswordView.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: SignInView,
    },
    {
      path: '/sign-in',
      name: 'sign-in',
      component: SignInView,
    },
    {
      path: '/sign-up',
      name: 'sign-up',
      component: SignUpView,
    },
    {
      path: '/reset-password',
      name: 'reset-password',
      component: ResetPasswordView,
    },
  ],
})

export default router
