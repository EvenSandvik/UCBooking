# UC Booking - Client

This is the client application for UC Booking, built with React, TypeScript, and Vite.

## Prerequisites

- Node.js (v16 or later)
- npm (v8 or later)

## Getting Started

### Installation

1. Clone the repository
2. Navigate to the client directory:
   ```bash
   cd packages/client
   ```
3. Install dependencies:
   ```bash
   npm install
   ```

## Available Scripts

In the project directory, you can run:

### `npm run dev`

Runs the app in development mode.\
Open [http://localhost:3000](http://localhost:3000) to view it in your browser.

### `npm run build`

Builds the app for production to the `dist` folder.\
The build is minified and the filenames include the hashes.

### `npm run preview`

Serves the production build from the `dist` folder.\
Open [http://localhost:4173](http://localhost:4173) to view it in your browser.

## Project Structure

```
client/
├── src/                  # Source files
│   ├── components/       # React components
│   ├── assets/           # Static assets
│   ├── App.tsx           # Main application component
│   └── main.tsx          # Application entry point
├── public/               # Public assets
├── dist/                 # Production build output
├── index.html            # Main HTML file
├── package.json          # Project dependencies and scripts
├── tsconfig.json         # TypeScript configuration
└── vite.config.ts        # Vite configuration
```

## Development

### Environment Variables

Create a `.env` file in the root of the client directory to set up environment-specific variables:

```env
VITE_API_URL=http://localhost:3001
```

### Code Style

This project uses ESLint and Prettier for code formatting. Run the following commands to check and fix code style issues:

```bash
# Check for linting errors
npm run lint

# Fix auto-fixable issues
npm run lint:fix
```

## Building for Production

To create a production build:

```bash
npm run build
```

This will create a `dist` directory containing the production build of your app.

## Serving the Production Build

To serve the production build locally:

```bash
# Build the application
npm run build

# Serve the built files
npm run preview
```

Then open [http://localhost:4173](http://localhost:4173) to view it in your browser.

## Deployment

To deploy the application, upload the contents of the `dist` directory to your web server or hosting service.

## Troubleshooting

### CORS Issues

If you encounter CORS issues during development:
1. Ensure the backend server is running and accessible
2. Check that the `VITE_API_URL` in your `.env` file is correct
3. Make sure the backend has CORS properly configured to accept requests from your development server

### Build Issues

If you encounter build issues:
1. Delete the `node_modules` folder and `package-lock.json`
2. Run `npm install` to reinstall dependencies
3. Try building again with `npm run build`
