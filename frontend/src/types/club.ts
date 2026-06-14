export type Club = {
  id: number;
  name: string;
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  province: string;
  postalCode: string | null;
  phoneNumber: string | null;
  latitude: number | null;
  longitude: number | null;
};

export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
};

export function formatFullAddress(club: Club): string {
  const parts = [club.addressLine1, club.addressLine2, club.city, club.province, club.postalCode];
  return parts.filter((part): part is string => Boolean(part)).join(', ');
}
