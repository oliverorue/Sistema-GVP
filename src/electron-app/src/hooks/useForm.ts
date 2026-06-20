import { useState, useCallback } from 'react'

interface UseFormOptions<T> {
  initialValues: T
  onSubmit: (values: T) => Promise<void>
  validate?: (values: T) => Partial<Record<keyof T, string>>
}

export function useForm<T extends Record<string, any>>({ initialValues, onSubmit, validate }: UseFormOptions<T>) {
  const [values, setValues] = useState<T>(initialValues)
  const [errors, setErrors] = useState<Partial<Record<keyof T, string>>>({})
  const [submitting, setSubmitting] = useState(false)

  const handleChange = useCallback((name: keyof T, value: any) => {
    setValues((prev) => ({ ...prev, [name]: value }))
    if (errors[name]) {
      setErrors((prev) => {
        const copy = { ...prev }
        delete copy[name]
        return copy
      })
    }
  }, [errors])

  const handleSubmit = useCallback(async () => {
    if (validate) {
      const validationErrors = validate(values)
      if (Object.keys(validationErrors).length > 0) {
        setErrors(validationErrors)
        return
      }
    }

    setSubmitting(true)
    try {
      await onSubmit(values)
    } finally {
      setSubmitting(false)
    }
  }, [values, validate, onSubmit])

  const reset = useCallback(() => {
    setValues(initialValues)
    setErrors({})
  }, [initialValues])

  return { values, errors, submitting, handleChange, handleSubmit, reset, setValues }
}
