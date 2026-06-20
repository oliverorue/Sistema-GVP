import { useState } from 'react'

interface PaginationState {
  page: number
  pageSize: number
  totalCount: number
}

export function usePagination(initialPageSize = 25) {
  const [pagination, setPagination] = useState<PaginationState>({
    page: 1,
    pageSize: initialPageSize,
    totalCount: 0,
  })

  const setPage = (page: number) => setPagination((prev) => ({ ...prev, page }))
  const setPageSize = (pageSize: number) => setPagination((prev) => ({ ...prev, pageSize, page: 1 }))
  const setTotalCount = (totalCount: number) => setPagination((prev) => ({ ...prev, totalCount }))

  const totalPages = Math.ceil(pagination.totalCount / pagination.pageSize)

  return {
    ...pagination,
    totalPages,
    setPage,
    setPageSize,
    setTotalCount,
  }
}
