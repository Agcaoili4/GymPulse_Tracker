import type { Club, PagedResult } from '../types/club';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5279';

export type GetClubsParams = {
  search?: string;
  city?: string;
  province?: string;
  page?: number;
  pageSize?: number;
};

export async function getClubs(params: GetClubsParams = {}, signal?: AbortSignal): Promise<PagedResult<Club>> {
  const query = new URLSearchParams();
  if (params.search) query.set('search', params.search);
  if (params.city) query.set('city', params.city);
  if (params.province) query.set('province', params.province);
  if (params.page) query.set('page', String(params.page));
  if (params.pageSize) query.set('pageSize', String(params.pageSize));

  const url = `${API_BASE_URL}/api/clubs${query.size > 0 ? `?${query}` : ''}`;
  const response = await fetch(url, { signal });
  if (!response.ok) {
    throw new Error(`GET /api/clubs failed with ${response.status}`);
  }
  return (await response.json()) as PagedResult<Club>;
}
