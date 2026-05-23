export type LocationStatus = 'low' | 'moderate' | 'busy';

export type TrendDirection = 'up' | 'down' | 'flat';

export type Location = {
  id: string;
  name: string;
  address: string;
  occupancy: number;
  capacity: number;
  trend: TrendDirection;
  lastUpdatedMinutesAgo: number;
};

export function getStatus(occupancy: number, capacity: number): LocationStatus {
  const ratio = occupancy / capacity;
  if (ratio >= 0.75) return 'busy';
  if (ratio >= 0.5) return 'moderate';
  return 'low';
}
