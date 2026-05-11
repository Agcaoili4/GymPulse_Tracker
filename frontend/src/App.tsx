import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from './assets/vite.svg'

function App() {
  const [count, setCount] = useState(0)

  return (
    <main className="min-h-screen bg-slate-50 px-6 py-10 text-slate-900 sm:px-10">
      <section className="mx-auto flex w-full max-w-4xl flex-col gap-10 rounded-[2rem] border border-slate-200 bg-white/95 p-8 shadow-lg shadow-slate-200/50 backdrop-blur-sm sm:p-12">
        <div className="flex flex-col gap-6 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-sm uppercase tracking-[0.24em] text-slate-500">GoodLife Fitness Tracker</p>
            <h1 className="mt-3 text-4xl font-semibold tracking-tight text-slate-900 sm:text-5xl">
              React + Vite + Tailwind CSS
            </h1>
            <p className="mt-4 max-w-2xl text-base leading-7 text-slate-600 sm:text-lg">
              A modern frontend setup for your fitness tracker using Vite, React, and Tailwind CSS.
            </p>
          </div>

          <div className="flex items-center gap-4">
            <img src={viteLogo} alt="Vite logo" className="h-14 w-14" />
            <img src={reactLogo} alt="React logo" className="h-14 w-14" />
          </div>
        </div>

        <div className="rounded-3xl border border-slate-200 bg-slate-50 p-6 sm:p-8">
          <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
            <div>
              <p className="text-sm font-medium text-slate-500">Starter counter</p>
              <p className="text-3xl font-semibold text-slate-900">Count is {count}</p>
            </div>
            <button
              type="button"
              onClick={() => setCount((value) => value + 1)}
              className="inline-flex items-center justify-center rounded-full bg-slate-900 px-5 py-3 text-sm font-semibold text-white transition hover:bg-slate-700 focus:outline-none focus:ring-2 focus:ring-slate-900/50"
            >
              Add one
            </button>
          </div>
        </div>

        <div className="grid gap-6 sm:grid-cols-2">
          <article className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
            <h2 className="text-xl font-semibold text-slate-900">Build fast</h2>
            <p className="mt-3 text-slate-600">
              Vite delivers instant server startup and fast HMR so your frontend development stays smooth.
            </p>
          </article>
          <article className="rounded-3xl border border-slate-200 bg-white p-6 shadow-sm">
            <h2 className="text-xl font-semibold text-slate-900">Style quickly</h2>
            <p className="mt-3 text-slate-600">
              Tailwind CSS makes responsive styling easy with utility-first classes and a clean design system.
            </p>
          </article>
        </div>
      </section>
    </main>
  )
}

export default App
