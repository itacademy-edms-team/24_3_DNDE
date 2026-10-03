import { useNavigate } from 'react-router';
import { Box, Card, CardActionArea, CardContent, Typography } from '@mui/material';
import Grid2 from '@mui/material/Grid2';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutline';

function ImportPdfIndexPage() {
  const navigate = useNavigate();

  return (
    <Box sx={{ p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Импорт транзакций из PDF
      </Typography>
      <Typography variant="body1" color="text.secondary" sx={{ mb: 4 }}>
        Выберите, куда импортировать транзакции
      </Typography>

      <Grid2 container spacing={3} sx={{ maxWidth: 600 }}>
        <Grid2 size={{ xs: 12, sm: 6 }}>
          <Card variant="outlined" sx={{ height: '100%' }}>
            <CardActionArea
              onClick={() => navigate('/import/existing-wallet')}
              sx={{ height: '100%', p: 1 }}
            >
              <CardContent sx={{ textAlign: 'center', py: 4 }}>
                <AccountBalanceWalletIcon sx={{ fontSize: 48, color: 'primary.main', mb: 2 }} />
                <Typography variant="h6" gutterBottom>
                  В существующий кошелёк
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Импортировать транзакции в один из ваших кошельков
                </Typography>
              </CardContent>
            </CardActionArea>
          </Card>
        </Grid2>

        <Grid2 size={{ xs: 12, sm: 6 }}>
          <Card variant="outlined" sx={{ height: '100%' }}>
            <CardActionArea
              onClick={() => navigate('/import/new-wallet')}
              sx={{ height: '100%', p: 1 }}
            >
              <CardContent sx={{ textAlign: 'center', py: 4 }}>
                <AddCircleOutlineIcon sx={{ fontSize: 48, color: 'primary.main', mb: 2 }} />
                <Typography variant="h6" gutterBottom>
                  В новый кошелёк
                </Typography>
                <Typography variant="body2" color="text.secondary">
                  Создать новый кошелёк и импортировать транзакции
                </Typography>
              </CardContent>
            </CardActionArea>
          </Card>
        </Grid2>
      </Grid2>
    </Box>
  );
}

export default ImportPdfIndexPage;
