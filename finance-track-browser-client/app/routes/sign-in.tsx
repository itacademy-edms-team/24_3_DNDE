import * as React from 'react';
import Container from '@mui/material/Container';
import Typography from '@mui/material/Typography';
import Box from '@mui/material/Box';
import { Field } from '@tanstack/react-form';
import { TextField } from '@mui/material';

export function meta() {
  return [
    { title: 'Sign-in' },
    {
      name: 'description',
      content: 'Welcome to Material UI - React Router example in TypeScript!',
    },
  ];
}

export default function SignIn() {
  return (
    <Container maxWidth="md">
      <Typography>Вход</Typography>
    </Container>
  );
}
