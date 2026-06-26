/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        'brand-red': {
          DEFAULT: '#E60000',
          light: '#FF1A1A',
          dark: '#CC0000',
          muted: '#4D0000',
        },
        'brand-black': '#000000',
        'brand-dark': '#0A0A0A',
        'brand-surface': '#111111',
        'brand-border': '#1E1E1E',
        'brand-muted': '#3D3D3D',
        'brand-gray': '#666666',
      },
      fontFamily: {
        // Named utilities - mapped to new premium fonts for backward compatibility
        'montserrat': ['Outfit', 'system-ui', 'sans-serif'],
        'inter':      ['Plus Jakarta Sans', 'system-ui', 'sans-serif'],
        'outfit':     ['Outfit', 'system-ui', 'sans-serif'],
        'plus-jakarta': ['Plus Jakarta Sans', 'system-ui', 'sans-serif'],

        // Semantic aliases
        'display': ['Outfit', 'system-ui', 'sans-serif'],
        'sans':    ['Plus Jakarta Sans', 'system-ui', 'sans-serif'],
        'mono':    ['IBM Plex Mono', 'ui-monospace', 'monospace'],
      },
      animation: {
        'fade-up':       'fadeUp 0.5s ease-out forwards',
        'fade-in':       'fadeIn 0.4s ease-out forwards',
        'slide-in-left': 'slideInLeft 0.4s ease-out forwards',
        'pulse-red':     'pulseRed 2s ease-in-out infinite',
        'spin-slow':     'spin 8s linear infinite',
        'slide-progress':'slideProgress 6s linear forwards',
      },
      keyframes: {
        fadeUp: {
          '0%':   { opacity: '0', transform: 'translateY(24px)' },
          '100%': { opacity: '1', transform: 'translateY(0)' },
        },
        fadeIn: {
          '0%':   { opacity: '0' },
          '100%': { opacity: '1' },
        },
        slideInLeft: {
          '0%':   { opacity: '0', transform: 'translateX(-24px)' },
          '100%': { opacity: '1', transform: 'translateX(0)' },
        },
        pulseRed: {
          '0%, 100%': { boxShadow: '0 0 0 0 rgba(230,0,0,0.4)' },
          '50%':      { boxShadow: '0 0 0 12px rgba(230,0,0,0)' },
        },
        slideProgress: {
          '0%': { width: '0%' },
          '100%': { width: '100%' }
        },
      },
    },
  },
  plugins: [],
}
