import * as React from 'react';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import Link from '@mui/material/Link';
import UnstyledLink from '~/components/UnstyledLink';
import ProTip from '~/components/ProTip';
import Copyright from '~/components/Copyright';
import { Button } from '@mui/material';
import { useState } from 'react';

export function meta() {
  return [
    { title: 'Material UI - React Router example in TypeScript' },
    {
      name: 'description',
      content: 'Welcome to Material UI - React Router example in TypeScript!',
    },
  ];
}

export default function Home() {
  return (
    <Container maxWidth="md">
      <Box
        sx={{
          mx: 2,
          my: 2,
          minHeight: '60vh',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          alignItems: 'center',
        }}
      >
        <Typography variant="h4" component="h1" sx={{ my: 2 }}>
          FinanceTrack - ваш верный друг в планировании финансов
        </Typography>
        <Button size="large" variant="contained">
          <UnstyledLink to="/sign-in">Попробовать</UnstyledLink>
        </Button>
      </Box>
    </Container>
  );
}
