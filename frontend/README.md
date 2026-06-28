# Frontend

The GymPulse frontend uses React, Vite, TypeScript, TSX, and Tailwind CSS.

## Scripts

```bash
npm run dev
npm start
npm run build
npm run lint
```

## TypeScript

- Application components use `.tsx`.
- Frontend configuration uses `.ts`.
- Production builds run `tsc --noEmit` before `vite build`.

## Tailwind CSS

Tailwind is configured in `tailwind.config.ts` and scans:

```text
./index.html
./src/**/*.{ts,tsx}
```
