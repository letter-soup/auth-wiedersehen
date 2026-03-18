import { onMounted, onUnmounted } from 'vue'

export function useWindowEventListener<T extends keyof WindowEventMap>(
  event: T,
  callback: (this: Window, ev: WindowEventMap[T]) => void,
  options?: boolean | AddEventListenerOptions,
): void {
  onMounted(() => window.addEventListener(event, callback, options))
  onUnmounted(() => window.removeEventListener(event, callback, options))
}
