import { ref } from 'vue';

type ToastType = 'success' | 'error';

export type ToastState = {
  message: string;
  type: ToastType;
};

export function useToast() {
  const toast = ref<ToastState | null>(null);
  let timeoutId: ReturnType<typeof window.setTimeout> | undefined;

  function showToast(message: string, type: ToastType) {
    if (timeoutId) {
      window.clearTimeout(timeoutId);
    }

    toast.value = { message, type };

    timeoutId = window.setTimeout(() => {
      toast.value = null;
      timeoutId = undefined;
    }, 3500);
  }

  function showSuccess(message: string) {
    showToast(message, 'success');
  }

  function showError(message: string) {
    showToast(message, 'error');
  }

  return {
    toast,
    showSuccess,
    showError
  };
}
