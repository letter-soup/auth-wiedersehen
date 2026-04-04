import { describe, it, expect, vi, afterEach } from 'vitest'
import { flushPromises } from '@vue/test-utils'
import { mount } from '@/lib/__tests__/mount'
import SignUpView from './SignUpView.vue'
import { InputStub, SeparatorStub, FormMessageStub } from '@/lib/__mock__/stubs'
import { createStub } from '@/lib/__mock__/component-stub'
import { createUser } from '@/lib/api/endpoints.ts'

vi.mock('vue-sonner', () => ({
  toast: vi.fn(),
}))

vi.mock('@/lib/api', () => ({
  checkEmailAvailability: vi.fn().mockResolvedValue(undefined),
  createUser: vi.fn().mockResolvedValue({ userId: '123', redirectUri: undefined }),
}))

vi.mock('@/lib/api/endpoints.ts', () => ({
  checkEmailAvailability: vi.fn().mockResolvedValue(undefined),
  createUser: vi.fn().mockResolvedValue({ userId: '123', redirectUri: undefined }),
}))

vi.mock('@/views/sign-up/lib/validate-email', () => ({
  validateEmail: async (
    _values: Record<string, string>,
    _validate: unknown,
    nextStep: () => void,
  ) => {
    nextStep()
  },
}))

const stubs = {
  Button: createStub('Button', { template: '<button type="button"><slot /></button>' }),
  Input: InputStub,
  Separator: SeparatorStub,
  FormMessage: FormMessageStub,
}

describe('SignUpView (snapshot)', () => {
  it('renders step 1 correctly (snapshot)', () => {
    const wrapper = mount(SignUpView, { global: { stubs } })
    expect(wrapper.html()).toMatchSnapshot()
  })
})

describe('SignUpView', () => {
  it('step 1 shows email input and CTA, step 2 hidden', () => {
    const wrapper = mount(SignUpView, { global: { stubs } })
    expect(wrapper.find('[data-testid="sign-up-step-email"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="sign-up-step-password"]').exists()).toBe(false)
    expect(wrapper.find('#email').exists()).toBe(true)
    expect(wrapper.find('#cta').exists()).toBe(true)
  })

  it('entering valid email and clicking CTA advances to step 2', async () => {
    const wrapper = mount(SignUpView, { global: { stubs } })

    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#cta').trigger('click')
    await flushPromises()

    expect(wrapper.find('[data-testid="sign-up-step-email"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="sign-up-step-password"]').exists()).toBe(true)
    expect(wrapper.find('#password').exists()).toBe(true)
    expect(wrapper.find('#confirm-password').exists()).toBe(true)
  })

  it('step 2 shows submit button and acknowledgement text with terms/privacy links', async () => {
    const wrapper = mount(SignUpView, { global: { stubs } })

    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#cta').trigger('click')
    await flushPromises()

    expect(wrapper.text()).toContain('sign-up:cta')
    expect(wrapper.html()).toContain('sign-up:cta-acknowledgement')
  })
})

describe('SignUpView OAuth2 query params', () => {
  it('preserves client_id and redirect_uri query params when advancing to step 2', async () => {
    const { createRouter, createMemoryHistory } = await import('vue-router')
    const router = createRouter({
      history: createMemoryHistory(),
      routes: [{ path: '/sign-up', name: 'sign-up', component: SignUpView }],
    })
    await router.push({ path: '/sign-up', query: { client_id: 'my-client', redirect_uri: 'https://app.example.com/cb' } })
    await router.isReady()

    const pushSpy = vi.spyOn(router, 'push')

    const wrapper = mount(SignUpView, {
      global: {
        stubs,
        plugins: [router],
      },
    })

    await wrapper.find('#email').setValue('test@example.com')
    await wrapper.find('#cta').trigger('click')
    await flushPromises()

    expect(pushSpy).toHaveBeenCalledWith(
      expect.objectContaining({
        query: expect.objectContaining({
          client_id: 'my-client',
          redirect_uri: 'https://app.example.com/cb',
        }),
      }),
    )
  })

  it('calls createUser with correct clientId and redirectUri on step 2 submit', async () => {
    const mockedCreateUser = vi.mocked(createUser)
    mockedCreateUser.mockResolvedValue({ userId: '456', redirectUri: undefined })

    const { createRouter, createMemoryHistory } = await import('vue-router')
    const router = createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/sign-up', name: 'sign-up', component: SignUpView },
        { path: '/', name: 'home', component: { template: '<div />' } },
      ],
    })
    await router.push({
      path: '/sign-up',
      query: { client_id: 'test-client', redirect_uri: 'https://example.com/callback' },
    })
    await router.isReady()

    const FormStub = {
      template: `<slot :values="values" :validate="validate" :setFieldError="setFieldError" />`,
      data() {
        return {
          values: { email: 'user@test.com', password: 'Password1!', confirmPassword: 'Password1!' },
        }
      },
      methods: {
        validate: () => Promise.resolve({ valid: true, errors: {} }),
        setFieldError: () => {},
      },
    }

    const wrapper = mount(SignUpView, {
      global: {
        stubs: { ...stubs, Form: FormStub },
        plugins: [router],
      },
    })

    // Advance to step 2
    await wrapper.find('#email').setValue('user@test.com')
    await wrapper.find('#cta').trigger('click')
    await flushPromises()

    // Submit the form on step 2
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(mockedCreateUser).toHaveBeenCalledWith(
      'user@test.com',
      'Password1!',
      true,
      'test-client',
      'https://example.com/callback',
    )
  })
})

describe('SignUpView redirect after createUser', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  async function mountWithFormStub(
    router: ReturnType<typeof import('vue-router').createRouter>,
    extraStubs?: Record<string, unknown>,
  ) {
    const FormStub = {
      template: `<slot :values="values" :validate="validate" :setFieldError="setFieldError" />`,
      data() {
        return {
          values: { email: 'user@test.com', password: 'Password1!', confirmPassword: 'Password1!' },
        }
      },
      methods: {
        validate: () => Promise.resolve({ valid: true, errors: {} }),
        setFieldError: () => {},
      },
    }

    const wrapper = mount(SignUpView, {
      global: {
        stubs: { ...stubs, Form: FormStub, ...extraStubs },
        plugins: [router],
      },
    })

    // Advance to step 2
    await wrapper.find('#email').setValue('user@test.com')
    await wrapper.find('#cta').trigger('click')
    await flushPromises()

    // Submit the form
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    return wrapper
  }

  it('redirects to redirectUri via window.location.href when createUser returns one', async () => {
    const mockedCreateUser = vi.mocked(createUser)
    mockedCreateUser.mockResolvedValue({
      userId: '789',
      redirectUri: 'https://consumer-app.com/dashboard',
    })

    const locationSpy = { href: '' }
    vi.stubGlobal('location', locationSpy)

    const { createRouter, createMemoryHistory } = await import('vue-router')
    const router = createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/sign-up', name: 'sign-up', component: SignUpView },
        { path: '/', name: 'home', component: { template: '<div />' } },
      ],
    })
    await router.push('/sign-up')
    await router.isReady()

    await mountWithFormStub(router)

    expect(locationSpy.href).toBe('https://consumer-app.com/dashboard')
  })

  it('redirects to / via router.push when createUser returns no redirectUri', async () => {
    const mockedCreateUser = vi.mocked(createUser)
    mockedCreateUser.mockResolvedValue({ userId: '101', redirectUri: undefined })

    const { createRouter, createMemoryHistory } = await import('vue-router')
    const router = createRouter({
      history: createMemoryHistory(),
      routes: [
        { path: '/sign-up', name: 'sign-up', component: SignUpView },
        { path: '/', name: 'home', component: { template: '<div />' } },
      ],
    })
    await router.push('/sign-up')
    await router.isReady()

    const pushSpy = vi.spyOn(router, 'push')

    await mountWithFormStub(router)

    expect(pushSpy).toHaveBeenCalledWith('/')
  })
})
