import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid2,
  IconButton,
  Stack,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TextField,
  Typography,
  Paper,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';

import Loading from '@/components/Loading';

const getErrorMessage = (operation: string, status: number): string => {
  return `Ошибка ${operation}: ${status}`;
};

type Category = {
  id: string;
  name: string;
  type: 'Income' | 'Expense';
  icon: string | null;
  color: string | null;
};

type CategoriesResponse = {
  categories: Category[];
};

type CreateCategoryPayload = {
  name: string;
  type: 'Income' | 'Expense';
  icon?: string | null;
  color?: string | null;
};

type UpdateCategoryPayload = {
  name: string;
  icon: string | null;
  color: string | null;
};

const fetchCategories = async (type?: 'Income' | 'Expense'): Promise<Category[]> => {
  const params = new URLSearchParams();
  if (type) params.append('Type', type);
  
  const url = `/api/finance/Categories${params.toString() ? `?${params.toString()}` : ''}`;
  const res = await fetch(url, {
    credentials: 'include',
  });
  if (!res.ok) {
    throw new Error(getErrorMessage('загрузки категорий', res.status));
  }
  const data: CategoriesResponse = await res.json();
  return data.categories;
};

const createCategory = async (payload: CreateCategoryPayload): Promise<{ id: string }> => {
  const res = await fetch('/api/finance/Categories', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('создания категории', res.status));
  }

  return await res.json();
};

const updateCategory = async (categoryId: string, payload: UpdateCategoryPayload): Promise<Category> => {
  const res = await fetch(`/api/finance/Categories/${categoryId}`, {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    credentials: 'include',
    body: JSON.stringify(payload),
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('обновления категории', res.status));
  }

  return await res.json();
};

const deleteCategory = async (categoryId: string): Promise<void> => {
  const res = await fetch(`/api/finance/Categories/${categoryId}`, {
    method: 'DELETE',
    credentials: 'include',
  });

  if (!res.ok) {
    throw new Error(getErrorMessage('удаления категории', res.status));
  }
};

type CategoryFormState = {
  name: string;
  icon: string;
  color: string;
};

function CategoriesPage() {
  const queryClient = useQueryClient();
  const [incomeDialogOpen, setIncomeDialogOpen] = useState(false);
  const [expenseDialogOpen, setExpenseDialogOpen] = useState(false);
  const [deleteConfirmOpen, setDeleteConfirmOpen] = useState(false);
  const [categoryToDelete, setCategoryToDelete] = useState<Category | null>(null);
  const [categoryToEdit, setCategoryToEdit] = useState<Category | null>(null);
  const [errorDialog, setErrorDialog] = useState({ open: false, message: '' });
  const [incomeForm, setIncomeForm] = useState<CategoryFormState>({
    name: '',
    icon: '',
    color: '',
  });
  const [expenseForm, setExpenseForm] = useState<CategoryFormState>({
    name: '',
    icon: '',
    color: '',
  });

  const { data: incomeCategories = [], isLoading: isLoadingIncome } = useQuery({
    queryKey: ['categories', 'Income'],
    queryFn: () => fetchCategories('Income'),
    retry: false,
  });

  const { data: expenseCategories = [], isLoading: isLoadingExpense } = useQuery({
    queryKey: ['categories', 'Expense'],
    queryFn: () => fetchCategories('Expense'),
    retry: false,
  });

  const createCategoryMutation = useMutation({
    mutationFn: (payload: CreateCategoryPayload) => createCategory(payload),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['categories', variables.type] });
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      if (variables.type === 'Income') {
        setIncomeDialogOpen(false);
        setIncomeForm({ name: '', icon: '', color: '' });
      } else {
        setExpenseDialogOpen(false);
        setExpenseForm({ name: '', icon: '', color: '' });
      }
    },
    onError: (error: Error) => {
      console.error('Failed to create category:', error);
      setErrorDialog({ open: true, message: error.message || 'Ошибка создания категории' });
    },
  });

  const updateCategoryMutation = useMutation({
    mutationFn: ({ categoryId, payload }: { categoryId: string; payload: UpdateCategoryPayload }) =>
      updateCategory(categoryId, payload),
    onSuccess: (updatedCategory) => {
      queryClient.invalidateQueries({ queryKey: ['categories', updatedCategory.type] });
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      if (updatedCategory.type === 'Income') {
        setIncomeDialogOpen(false);
        setIncomeForm({ name: '', icon: '', color: '' });
      } else {
        setExpenseDialogOpen(false);
        setExpenseForm({ name: '', icon: '', color: '' });
      }
      setCategoryToEdit(null);
    },
    onError: (error: Error) => {
      console.error('Failed to update category:', error);
      setErrorDialog({ open: true, message: error.message || 'Ошибка обновления категории' });
    },
  });

  const deleteCategoryMutation = useMutation({
    mutationFn: deleteCategory,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['categories'] });
      setDeleteConfirmOpen(false);
      setCategoryToDelete(null);
    },
    onError: (error: Error) => {
      console.error('Failed to delete category:', error);
      setErrorDialog({ open: true, message: error.message || 'Ошибка удаления категории' });
    },
  });

  const handleOpenIncomeDialog = () => {
    setCategoryToEdit(null);
    setIncomeForm({ name: '', icon: '', color: '' });
    setIncomeDialogOpen(true);
  };

  const handleOpenExpenseDialog = () => {
    setCategoryToEdit(null);
    setExpenseForm({ name: '', icon: '', color: '' });
    setExpenseDialogOpen(true);
  };

  const handleEditCategory = (category: Category) => {
    setCategoryToEdit(category);
    const formState = { name: category.name, icon: category.icon || '', color: category.color || '' };
    if (category.type === 'Income') {
      setIncomeForm(formState);
      setIncomeDialogOpen(true);
    } else {
      setExpenseForm(formState);
      setExpenseDialogOpen(true);
    }
  };

  const handleSaveIncome = () => {
    if (!incomeForm.name.trim()) {
      setErrorDialog({ open: true, message: 'Введите название категории' });
      return;
    }

    if (categoryToEdit) {
      const payload: UpdateCategoryPayload = {
        name: incomeForm.name.trim(),
        icon: incomeForm.icon.trim() || null,
        color: incomeForm.color.trim() || null,
      };
      updateCategoryMutation.mutate({ categoryId: categoryToEdit.id, payload });
    } else {
      const payload: CreateCategoryPayload = {
        name: incomeForm.name.trim(),
        type: 'Income',
        icon: incomeForm.icon.trim() || null,
        color: incomeForm.color.trim() || null,
      };
      createCategoryMutation.mutate(payload);
    }
  };

  const handleSaveExpense = () => {
    if (!expenseForm.name.trim()) {
      setErrorDialog({ open: true, message: 'Введите название категории' });
      return;
    }

    if (categoryToEdit) {
      const payload: UpdateCategoryPayload = {
        name: expenseForm.name.trim(),
        icon: expenseForm.icon.trim() || null,
        color: expenseForm.color.trim() || null,
      };
      updateCategoryMutation.mutate({ categoryId: categoryToEdit.id, payload });
    } else {
      const payload: CreateCategoryPayload = {
        name: expenseForm.name.trim(),
        type: 'Expense',
        icon: expenseForm.icon.trim() || null,
        color: expenseForm.color.trim() || null,
      };
      createCategoryMutation.mutate(payload);
    }
  };

  const handleDeleteClick = (category: Category) => {
    setCategoryToDelete(category);
    setDeleteConfirmOpen(true);
  };

  const handleDeleteConfirm = () => {
    if (categoryToDelete) {
      deleteCategoryMutation.mutate(categoryToDelete.id);
    }
  };

  if (isLoadingIncome || isLoadingExpense) {
    return <Loading />;
  }

  return (
    <Box sx={{ p: 3 }}>
      <meta name="title" content="Категории" />
      <Typography variant="h4" sx={{ mb: 3 }}>
        Категории
      </Typography>

      <Grid2 container spacing={3}>
        {/* Блок доходов */}
        <Grid2 size={{ xs: 12, md: 6 }}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <TrendingUpIcon color="success" />
                  <Typography variant="h6">Доходы</Typography>
                </Box>
                <Button
                  variant="contained"
                  color="success"
                  startIcon={<AddIcon />}
                  onClick={handleOpenIncomeDialog}
                  size="small"
                >
                  Добавить
                </Button>
              </Box>

              {incomeCategories.length === 0 ? (
                <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 3 }}>
                  Нет категорий доходов. Добавьте первую категорию.
                </Typography>
              ) : (
                <TableContainer component={Paper} variant="outlined">
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Название</TableCell>
                        <TableCell>Иконка</TableCell>
                        <TableCell>Цвет</TableCell>
                        <TableCell align="right">Действия</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {incomeCategories.map((category) => (
                        <TableRow key={category.id}>
                          <TableCell>{category.name}</TableCell>
                          <TableCell>{category.icon || '-'}</TableCell>
                          <TableCell>
                            {category.color ? (
                              <Box
                                sx={{
                                  width: 24,
                                  height: 24,
                                  borderRadius: '50%',
                                  bgcolor: category.color,
                                  border: '1px solid',
                                  borderColor: 'divider',
                                }}
                              />
                            ) : (
                              '-'
                            )}
                          </TableCell>
                          <TableCell align="right">
                            <Box sx={{ display: 'flex', gap: 0.5, justifyContent: 'flex-end' }}>
                              <IconButton
                                size="small"
                                color="primary"
                                onClick={() => handleEditCategory(category)}
                              >
                                <EditIcon fontSize="small" />
                              </IconButton>
                              <IconButton
                                size="small"
                                color="error"
                                onClick={() => handleDeleteClick(category)}
                              >
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </Box>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </CardContent>
          </Card>
        </Grid2>

        {/* Блок расходов */}
        <Grid2 size={{ xs: 12, md: 6 }}>
          <Card variant="outlined">
            <CardContent>
              <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                  <TrendingDownIcon color="error" />
                  <Typography variant="h6">Расходы</Typography>
                </Box>
                <Button
                  variant="contained"
                  color="error"
                  startIcon={<AddIcon />}
                  onClick={handleOpenExpenseDialog}
                  size="small"
                >
                  Добавить
                </Button>
              </Box>

              {expenseCategories.length === 0 ? (
                <Typography variant="body2" color="text.secondary" sx={{ textAlign: 'center', py: 3 }}>
                  Нет категорий расходов. Добавьте первую категорию.
                </Typography>
              ) : (
                <TableContainer component={Paper} variant="outlined">
                  <Table size="small">
                    <TableHead>
                      <TableRow>
                        <TableCell>Название</TableCell>
                        <TableCell>Иконка</TableCell>
                        <TableCell>Цвет</TableCell>
                        <TableCell align="right">Действия</TableCell>
                      </TableRow>
                    </TableHead>
                    <TableBody>
                      {expenseCategories.map((category) => (
                        <TableRow key={category.id}>
                          <TableCell>{category.name}</TableCell>
                          <TableCell>{category.icon || '-'}</TableCell>
                          <TableCell>
                            {category.color ? (
                              <Box
                                sx={{
                                  width: 24,
                                  height: 24,
                                  borderRadius: '50%',
                                  bgcolor: category.color,
                                  border: '1px solid',
                                  borderColor: 'divider',
                                }}
                              />
                            ) : (
                              '-'
                            )}
                          </TableCell>
                          <TableCell align="right">
                            <Box sx={{ display: 'flex', gap: 0.5, justifyContent: 'flex-end' }}>
                              <IconButton
                                size="small"
                                color="primary"
                                onClick={() => handleEditCategory(category)}
                              >
                                <EditIcon fontSize="small" />
                              </IconButton>
                              <IconButton
                                size="small"
                                color="error"
                                onClick={() => handleDeleteClick(category)}
                              >
                                <DeleteIcon fontSize="small" />
                              </IconButton>
                            </Box>
                          </TableCell>
                        </TableRow>
                      ))}
                    </TableBody>
                  </Table>
                </TableContainer>
              )}
            </CardContent>
          </Card>
        </Grid2>
      </Grid2>

      {/* Dialog создания/редактирования категории доходов */}
      <Dialog open={incomeDialogOpen} onClose={() => setIncomeDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {categoryToEdit ? 'Редактировать категорию дохода' : 'Добавить категорию дохода'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <TextField
              label="Название"
              value={incomeForm.name}
              onChange={(e) => setIncomeForm({ ...incomeForm, name: e.target.value })}
              fullWidth
              required
              autoFocus
            />
            <TextField
              label="Иконка (опционально)"
              value={incomeForm.icon}
              onChange={(e) => setIncomeForm({ ...incomeForm, icon: e.target.value })}
              fullWidth
              placeholder="Например: 🍕 или restaurant"
              helperText="Emoji или название иконки"
            />
            <TextField
              label="Цвет (опционально)"
              value={incomeForm.color}
              onChange={(e) => setIncomeForm({ ...incomeForm, color: e.target.value })}
              fullWidth
              placeholder="Например: #FF5722 или red"
              helperText="Hex-код цвета или название"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setIncomeDialogOpen(false)}>Отмена</Button>
          <Button
            onClick={handleSaveIncome}
            variant="contained"
            color="success"
            disabled={createCategoryMutation.isPending || updateCategoryMutation.isPending}
          >
            {categoryToEdit
              ? updateCategoryMutation.isPending
                ? 'Сохранение...'
                : 'Сохранить'
              : createCategoryMutation.isPending
                ? 'Создание...'
                : 'Создать'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Dialog создания/редактирования категории расходов */}
      <Dialog open={expenseDialogOpen} onClose={() => setExpenseDialogOpen(false)} maxWidth="sm" fullWidth>
        <DialogTitle>
          {categoryToEdit ? 'Редактировать категорию расхода' : 'Добавить категорию расхода'}
        </DialogTitle>
        <DialogContent>
          <Stack spacing={3} sx={{ mt: 1 }}>
            <TextField
              label="Название"
              value={expenseForm.name}
              onChange={(e) => setExpenseForm({ ...expenseForm, name: e.target.value })}
              fullWidth
              required
              autoFocus
            />
            <TextField
              label="Иконка (опционально)"
              value={expenseForm.icon}
              onChange={(e) => setExpenseForm({ ...expenseForm, icon: e.target.value })}
              fullWidth
              placeholder="Например: 🍕 или restaurant"
              helperText="Emoji или название иконки"
            />
            <TextField
              label="Цвет (опционально)"
              value={expenseForm.color}
              onChange={(e) => setExpenseForm({ ...expenseForm, color: e.target.value })}
              fullWidth
              placeholder="Например: #FF5722 или red"
              helperText="Hex-код цвета или название"
            />
          </Stack>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setExpenseDialogOpen(false)}>Отмена</Button>
          <Button
            onClick={handleSaveExpense}
            variant="contained"
            color="error"
            disabled={createCategoryMutation.isPending || updateCategoryMutation.isPending}
          >
            {categoryToEdit
              ? updateCategoryMutation.isPending
                ? 'Сохранение...'
                : 'Сохранить'
              : createCategoryMutation.isPending
                ? 'Создание...'
                : 'Создать'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Dialog подтверждения удаления */}
      <Dialog open={deleteConfirmOpen} onClose={() => setDeleteConfirmOpen(false)}>
        <DialogTitle>Удалить категорию</DialogTitle>
        <DialogContent>
          <Typography>
            Вы уверены, что хотите удалить категорию &quot;{categoryToDelete?.name}&quot;?
            <br />
            Транзакции с этой категорией не будут удалены, но категория станет недоступна.
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteConfirmOpen(false)}>Отмена</Button>
          <Button
            onClick={handleDeleteConfirm}
            color="error"
            variant="contained"
            disabled={deleteCategoryMutation.isPending}
            startIcon={<DeleteIcon />}
          >
            {deleteCategoryMutation.isPending ? 'Удаление...' : 'Удалить'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Error Dialog */}
      <ErrorDialog
        open={errorDialog.open}
        message={errorDialog.message}
        onClose={() => setErrorDialog({ open: false, message: '' })}
      />
    </Box>
  );
}

type ErrorDialogProps = {
  open: boolean;
  message: string;
  onClose: () => void;
};

function ErrorDialog({ open, message, onClose }: ErrorDialogProps) {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="xs" fullWidth>
      <DialogTitle>Ошибка</DialogTitle>
      <DialogContent dividers>
        <Typography variant="body2" sx={{ whiteSpace: 'pre-line' }}>
          {message}
        </Typography>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} autoFocus>
          Ок
        </Button>
      </DialogActions>
    </Dialog>
  );
}

export default CategoriesPage;
