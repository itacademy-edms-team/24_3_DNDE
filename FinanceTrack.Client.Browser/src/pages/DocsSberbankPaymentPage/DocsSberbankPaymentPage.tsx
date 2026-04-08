import { Alert, Box, Chip, Divider, Paper, Typography } from '@mui/material';
import ImageIcon from '@mui/icons-material/Image';

function DocsSberbankPaymentPage() {
  return (
    <Box sx={{ p: 3, maxWidth: 720 }}>
      <Typography variant="h4" gutterBottom>
        Сбербанк — выписка по платежам
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        Инструкция по определению типа PDF-файла для импорта транзакций
      </Typography>

      <Alert severity="info" sx={{ mb: 3 }}>
        В данный момент поддерживается импорт только из текстовых PDF (не сканированных).
      </Alert>

      <Divider sx={{ mb: 3 }} />

      <Typography variant="h6" gutterBottom>
        Идентификатор типа выписки
      </Typography>
      <Chip label="SberbankPayment000001" variant="outlined" sx={{ mb: 3, fontFamily: 'monospace', fontSize: '0.9rem' }} />

      <Typography variant="h6" gutterBottom>
        Как выглядит первая страница
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Первая страница выписки по платежам Сбербанка должна содержать заголовок с названием банка,
        период выписки и таблицу транзакций с колонками: дата, описание, сумма.
      </Typography>

      <Paper
        variant="outlined"
        sx={{
          width: '100%',
          aspectRatio: '210 / 297',
          maxWidth: 400,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          flexDirection: 'column',
          gap: 1,
          color: 'text.disabled',
          bgcolor: 'action.hover',
        }}
      >
        <ImageIcon sx={{ fontSize: 64 }} />
        <Typography variant="body2">Изображение будет добавлено позже</Typography>
      </Paper>
    </Box>
  );
}

export default DocsSberbankPaymentPage;
