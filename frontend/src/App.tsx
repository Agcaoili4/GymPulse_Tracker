import { useEffect, useState } from 'react';
import { getClubs } from './api/clubs';
import { ClubCard } from './components/ClubCard';
import type { Club } from './types/club';

type LoadState =
  | { status: 'loading' }
  | { status: 'error'; message: string }
  | { status: 'ready'; clubs: Club[]; totalCount: number };

function App() {
  const [state, setState] = useState<LoadState>({ status: 'loading' });

  useEffect(() => {
    const controller = new AbortController();

    getClubs({ pageSize: 50 }, controller.signal)
      .then((page) => {
        setState({ status: 'ready', clubs: page.items, totalCount: page.totalCount });
      })
      .catch((error: unknown) => {
        if (error instanceof DOMException && error.name === 'AbortError') return;
        const message = error instanceof Error ? error.message : 'Unknown error';
        setState({ status: 'error', message });
      });

    return () => controller.abort();
  }, []);

  return (
    <main className="min-h-screen bg-slate-50 px-6 py-10 sm:px-10">
      <div className="mx-auto max-w-7xl">
        <header className="mb-8">
          <h1 className="text-2xl font-semibold text-slate-900">
            {state.status === 'ready' ? `${state.totalCount} Clubs` : 'Clubs'}
          </h1>
          <p className="text-sm text-slate-500">All clubs across Calgary</p>
        </header>

        {state.status === 'loading' && <p className="text-sm text-slate-500">Loading clubs…</p>}

        {state.status === 'error' && (
          <div className="rounded-2xl border border-rose-200 bg-rose-50 p-6 text-sm text-rose-700">
            Failed to load clubs: {state.message}
            <p className="mt-2 text-rose-600/80">Is the API running at http://localhost:5279?</p>
          </div>
        )}

        {state.status === 'ready' && (
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
            {state.clubs.map((club) => (
              <ClubCard key={club.id} club={club} />
            ))}
          </div>
        )}
      </div>
    </main>
  );
}

export default App;
