import { Alert, Box, Chip, Divider, Paper, Typography } from '@mui/material';
import ImageIcon from '@mui/icons-material/Image';
import sberPayment000001 from '@/assets/docs/sberbank/payment/sberPayment000001.jpg';

type DocImageExample = {
  id: string;
  title: string;
  note: string;
  src?: string;
  alt?: string;
};

const examples: DocImageExample[] = [
  {
    id: 'SberbankPayment000001',
    title: 'Пример 1',
    note: 'Первая страница выписки по платежам Сбербанка.',
    src: sberPayment000001
  },
];

function DocsSberbankPaymentPage() {
  return (
    <Box sx={{ p: 3, maxWidth: 960 }}>
      <Typography variant="h4" gutterBottom>
        Сбербанк - выписка по платежам
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 3 }}>
        Информация по определению типа PDF-файла для импорта транзакций
      </Typography>

      <Alert severity="info" sx={{ mb: 3 }}>
        В данный момент поддерживается импорт только из текстовых PDF (не сканированных).
      </Alert>

      <Divider sx={{ mb: 3 }} />

      <Box sx={{ display: 'grid', gap: 3 }}>
        {examples.map((item) => (
          <Box key={item.id} component="figure" sx={{ m: 0 }}>
            <Typography variant="h6" gutterBottom>
              {item.id}
            </Typography>
            {item.src ? (
              <Box
                component="img"
                src={item.src}
                alt={item.alt || item.title}
                sx={{
                  width: '100%',
                  maxWidth: 700,
                  borderRadius: 1,
                  border: '1px solid',
                  borderColor: 'divider',
                  display: 'block',
                }}
              />
            ) : (
              <Paper
                variant="outlined"
                sx={{
                  width: '100%',
                  maxWidth: 560,
                  aspectRatio: '210 / 297',
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
            )}

            <Typography component="figcaption" variant="body2" color="text.secondary" sx={{ mt: 1 }}>
              {item.note}
            </Typography>
          </Box>
        ))}
      </Box>
    </Box>
  );
}

export default DocsSberbankPaymentPage;
