import type { Location } from '../types/location';

export const mockLocations: Location[] = [
  {
    id: 'downtown',
    name: 'GoodLife Fitness Calgary Downtown',
    address: '220 4th Ave SW',
    occupancy: 45,
    capacity: 150,
    trend: 'up',
    lastUpdatedMinutesAgo: 2,
  },
  {
    id: 'chinook',
    name: 'GoodLife Fitness Chinook',
    address: '6455 Macleod Trail SW',
    occupancy: 120,
    capacity: 180,
    trend: 'flat',
    lastUpdatedMinutesAgo: 1,
  },
  {
    id: 'crowfoot',
    name: 'GoodLife Fitness Crowfoot',
    address: '30 Crowfoot Terrace NW',
    occupancy: 165,
    capacity: 200,
    trend: 'up',
    lastUpdatedMinutesAgo: 3,
  },
];
