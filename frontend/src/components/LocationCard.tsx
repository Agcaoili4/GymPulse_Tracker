import type { Location, LocationStatus, TrendDirection } from '../types/location';
import { getStatus } from '../types/location';

type LocationCardProps = {
  location: Location;
};

export function LocationCard({ location }: LocationCardProps) {
  const status = getStatus(location.occupancy, location.capacity);
  const percentage = Math.round((location.occupancy / location.capacity) * 100);

  return (
    <article className="flex flex-col gap-4 rounded-2xl border border-slate-200 bg-white p-6 shadow-sm">
      <header className="flex items-start justify-between gap-3">
        <div>
          <h3 className="text-base font-semibold text-slate-900">{location.name}</h3>
          <p className="mt-1 flex items-center gap-1 text-sm text-slate-500">
            <MapPinIcon />
            {location.address}
          </p>
        </div>
        <StatusBadge status={status} />
      </header>

      <div className="flex flex-col gap-2">
        <div className="flex items-baseline justify-between">
          <span className="flex items-center gap-1 text-sm text-slate-500">
            <UsersIcon />
            Occupancy
          </span>
          <span className="flex items-baseline gap-1">
            <span className="text-lg font-semibold text-slate-900">{location.occupancy}</span>
            <span className="text-sm text-slate-400">/ {location.capacity}</span>
            <TrendIndicator direction={location.trend} />
          </span>
        </div>
        <OccupancyBar percentage={percentage} status={status} />
        <div className="flex items-center justify-between text-xs text-slate-400">
          <span>{location.lastUpdatedMinutesAgo} min ago</span>
          <span>{percentage}%</span>
        </div>
      </div>

      <button
        type="button"
        className="mt-auto flex items-center justify-center gap-2 rounded-xl border border-slate-200 bg-slate-50 px-4 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-slate-100"
      >
        <ChatIcon />
        Report Crowd Level
      </button>
    </article>
  );
}

function StatusBadge({ status }: { status: LocationStatus }) {
  const styles: Record<LocationStatus, string> = {
    low: 'border border-slate-200 bg-white text-slate-500',
    moderate: 'border border-rose-200 bg-white text-rose-500',
    busy: 'bg-rose-500 text-white',
  };
  const labels: Record<LocationStatus, string> = {
    low: 'Low Activity',
    moderate: 'Moderate',
    busy: 'Busy',
  };
  return (
    <span className={`inline-flex shrink-0 items-center rounded-full px-3 py-1 text-xs font-medium ${styles[status]}`}>
      {labels[status]}
    </span>
  );
}

function OccupancyBar({ percentage, status }: { percentage: number; status: LocationStatus }) {
  const fillColor: Record<LocationStatus, string> = {
    low: 'bg-slate-300',
    moderate: 'bg-rose-300',
    busy: 'bg-rose-500',
  };
  return (
    <div className="h-1.5 w-full overflow-hidden rounded-full bg-slate-100">
      <div
        className={`h-full rounded-full transition-all ${fillColor[status]}`}
        style={{ width: `${Math.min(percentage, 100)}%` }}
      />
    </div>
  );
}

function TrendIndicator({ direction }: { direction: TrendDirection }) {
  if (direction === 'flat') return null;
  return (
    <span className={direction === 'up' ? 'text-rose-500' : 'text-slate-400'} aria-label={`Trend ${direction}`}>
      {direction === 'up' ? '↗' : '↘'}
    </span>
  );
}

function MapPinIcon() {
  return (
    <svg viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M21 10c0 7-9 13-9 13s-9-6-9-13a9 9 0 0118 0z" />
      <circle cx="12" cy="10" r="3" />
    </svg>
  );
}

function UsersIcon() {
  return (
    <svg viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M17 21v-2a4 4 0 00-4-4H7a4 4 0 00-4 4v2" />
      <circle cx="10" cy="7" r="4" />
      <path strokeLinecap="round" strokeLinejoin="round" d="M21 21v-2a4 4 0 00-3-3.87M16 3.13a4 4 0 010 7.75" />
    </svg>
  );
}

function ChatIcon() {
  return (
    <svg viewBox="0 0 24 24" className="h-4 w-4" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden="true">
      <path strokeLinecap="round" strokeLinejoin="round" d="M21 15a2 2 0 01-2 2H7l-4 4V5a2 2 0 012-2h14a2 2 0 012 2z" />
    </svg>
  );
}
