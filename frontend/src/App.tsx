import { LocationCard } from './components/LocationCard';
import { mockLocations } from './mocks/locations';

function App() {
  return (
    <main className="min-h-screen bg-slate-50 px-6 py-10 sm:px-10">
      <div className="mx-auto max-w-7xl">
        <header className="mb-8">
          <h1 className="text-2xl font-semibold text-slate-900">{mockLocations.length} Locations</h1>
          <p className="text-sm text-slate-500">All locations across Calgary</p>
        </header>
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
          {mockLocations.map((loc) => (
            <LocationCard key={loc.id} location={loc} />
          ))}
        </div>
      </div>
    </main>
  );
}

export default App;
