import * as React from 'react';
import { createTheme, ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { Global } from '@emotion/react';

const theme = createTheme({
  cssVariables: true,
  colorSchemes: {
    light: true,
    dark: true,
  },
});

interface AppThemeProps {
  children: React.ReactNode;
}

export default function AppTheme({ children }: AppThemeProps) {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <Global
        styles={{
          a: {
            textDecoration: 'none',
            color: 'inherit',
          },
          'a:hover, a:focus, a:active': {
            textDecoration: 'none',
            color: 'inherit',
          },
        }}
      />
      {children}
    </ThemeProvider>
  );
}
