import { useCallback, useEffect, useMemo, useState } from 'react';
import ArrowBackIosNewIcon from '@mui/icons-material/ArrowBackIosNew';
import ArrowForwardIosIcon from '@mui/icons-material/ArrowForwardIos';
import {
  Box,
  Button,
  CircularProgress,
  MenuItem,
  Paper,
  Stack,
  TextField,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Chip,
} from '@mui/material';
import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';
import { fetchIncomes, fetchExpensesByIncome } from './api';
import type { ExpenseTransaction, IncomeTransaction } from './api';

const MONTH_OPTIONS = [
  { value: 1, label: 'Январь' },
  { value: 2, label: 'Февраль' },
  { value: 3, label: 'Март' },
  { value: 4, label: 'Апрель' },
  { value: 5, label: 'Май' },
  { value: 6, label: 'Июнь' },
  { value: 7, label: 'Июль' },
  { value: 8, label: 'Август' },
  { value: 9, label: 'Сентябрь' },
  { value: 10, label: 'Октябрь' },
  { value: 11, label: 'Ноябрь' },
  { value: 12, label: 'Декабрь' },
];

const getYearMonth = (dateStr: string): { year: number; month: number } => {
  const [yearRaw, monthRaw] = dateStr.split('-');
  return {
    year: Number(yearRaw) || 0,
    month: Number(monthRaw) || 0,
  };
};

const isInPeriod = (
  transactionDate: string,
  isMonthly: boolean,
  selectedYear: number,
  selectedMonth: number
): boolean => {
  const { year, month } = getYearMonth(transactionDate);
  if (!year || !month) return false;
  if (isMonthly) {
    return year < selectedYear || (year === selectedYear && month <= selectedMonth);
  }
  return year === selectedYear && month === selectedMonth;
};

const formatMoney = (value: number): string => `${value.toFixed(2)} ₽`;

// Цвета для секторов графика
const COLORS = [
  '#0088FE',
  '#00C49F',
  '#FFBB28',
  '#FF8042',
  '#8884D8',
  '#82CA9D',
  '#FFC658',
  '#FF7C7C',
  '#8DD1E1',
  '#D084D0',
  '#FFB347',
  '#87CEEB',
];

type ExpenseGroup = {
  name: string;
  value: number;
  count: number;
};

type BarChartData = {
  date: string;
  amount: number;
  count: number;
  label: string; // Для отображения на оси X
};

type DateRange = {
  startYear: number;
  startMonth: number;
  endYear: number;
  endMonth: number;
};

type MonthlyBalance = {
  year: number;
  month: number;
  incomes: number;
  expenses: number;
  balance: number;
};

function AnalyticsPage() {
  const now = useMemo(() => new Date(), []);
  // Фильтры для Pie Chart
  const [selectedYear, setSelectedYear] = useState<number>(now.getFullYear());
  const [selectedMonth, setSelectedMonth] = useState<number>(now.getMonth() + 1);
  // Фильтры для Bar Chart расходов (гистограммы) - диапазон дат
  const [barChartStartYear, setBarChartStartYear] = useState<number>(now.getFullYear());
  const [barChartStartMonth, setBarChartStartMonth] = useState<number>(now.getMonth() + 1);
  const [barChartEndYear, setBarChartEndYear] = useState<number>(now.getFullYear());
  const [barChartEndMonth, setBarChartEndMonth] = useState<number>(now.getMonth() + 1);
  // Фильтры для Bar Chart доходов (гистограммы) - диапазон дат
  const [incomeChartStartYear, setIncomeChartStartYear] = useState<number>(now.getFullYear());
  const [incomeChartStartMonth, setIncomeChartStartMonth] = useState<number>(now.getMonth() + 1);
  const [incomeChartEndYear, setIncomeChartEndYear] = useState<number>(now.getFullYear());
  const [incomeChartEndMonth, setIncomeChartEndMonth] = useState<number>(now.getMonth() + 1);
  const [incomes, setIncomes] = useState<IncomeTransaction[]>([]);
  const [expensesByIncome, setExpensesByIncome] = useState<Record<string, ExpenseTransaction[]>>({});
  const [loadingIncomes, setLoadingIncomes] = useState(false);
  const [loadingExpenses, setLoadingExpenses] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadExpenses = useCallback(async (incomeId: string) => {
    try {
      const data = await fetchExpensesByIncome(incomeId);
      setExpensesByIncome(prev => {
        if (prev[incomeId]) {
          return prev; // Уже загружены
        }
        return {
          ...prev,
          [incomeId]: data,
        };
      });
    } catch (e) {
      const err = e as Error;
      setError(err.message ?? 'Ошибка загрузки расходов');
    }
  }, []);

  const loadIncomes = useCallback(async () => {
    setLoadingIncomes(true);
    setError(null);
    try {
      const data = await fetchIncomes();
      setIncomes(data);
      
      // Загружаем расходы для всех доходов
      setLoadingExpenses(true);
      try {
        await Promise.all(data.map(income => loadExpenses(income.id)));
      } catch (e) {
        const err = e as Error;
        setError(err.message ?? 'Ошибка загрузки расходов');
      } finally {
        setLoadingExpenses(false);
      }
    } catch (e) {
      const err = e as Error;
      setError(err.message ?? 'Ошибка загрузки доходов');
    } finally {
      setLoadingIncomes(false);
    }
  }, [loadExpenses]);

  useEffect(() => {
    void loadIncomes();
  }, [loadIncomes]);

  // Собираем все расходы из всех доходов
  const allExpenses = useMemo(() => {
    return Object.values(expensesByIncome).flat();
  }, [expensesByIncome]);

  // Фильтруем расходы по выбранному периоду
  const filteredExpenses = useMemo(() => {
    return allExpenses.filter(e => isInPeriod(e.operationDate, e.isMonthly, selectedYear, selectedMonth));
  }, [allExpenses, selectedYear, selectedMonth]);

  // Группируем расходы по названиям
  const expenseGroups = useMemo<ExpenseGroup[]>(() => {
    const groups = new Map<string, number>();

    filteredExpenses.forEach(expense => {
      const current = groups.get(expense.name) || 0;
      groups.set(expense.name, current + expense.amount);
    });

    return Array.from(groups.entries())
      .map(([name, value]) => ({
        name,
        value: Number(value.toFixed(2)),
        count: filteredExpenses.filter(e => e.name === name).length,
      }))
      .sort((a, b) => b.value - a.value);
  }, [filteredExpenses]);

  const totalAmount = useMemo(
    () => expenseGroups.reduce((sum, group) => sum + group.value, 0),
    [expenseGroups]
  );

  const availableYears = useMemo(() => {
    const set = new Set<number>([now.getFullYear(), selectedYear]);
    incomes.forEach(i => {
      const { year } = getYearMonth(i.operationDate);
      if (year) set.add(year);
    });
    allExpenses.forEach(e => {
      const { year } = getYearMonth(e.operationDate);
      if (year) set.add(year);
    });
    return Array.from(set).sort((a, b) => b - a);
  }, [incomes, allExpenses, now, selectedYear]);

  // Получаем все месяцы, где есть транзакции
  const monthsWithTransactions = useMemo(() => {
    const monthSet = new Set<string>();
    
    incomes.forEach(income => {
      const { year, month } = getYearMonth(income.operationDate);
      if (year && month) {
        if (income.isMonthly) {
          // Для помесячных доходов добавляем все месяцы начиная с месяца создания до текущего
          const nowDate = new Date();
          let currentYear = year;
          let currentMonth = month;
          
          while (
            currentYear < nowDate.getFullYear() ||
            (currentYear === nowDate.getFullYear() && currentMonth <= nowDate.getMonth() + 1)
          ) {
            monthSet.add(`${currentYear}-${currentMonth}`);
            currentMonth++;
            if (currentMonth > 12) {
              currentMonth = 1;
              currentYear++;
            }
          }
        } else {
          monthSet.add(`${year}-${month}`);
        }
      }
    });

    allExpenses.forEach(expense => {
      const { year, month } = getYearMonth(expense.operationDate);
      if (year && month) {
        if (expense.isMonthly) {
          // Для помесячных расходов добавляем все месяцы начиная с месяца создания до текущего
          const nowDate = new Date();
          let currentYear = year;
          let currentMonth = month;
          
          while (
            currentYear < nowDate.getFullYear() ||
            (currentYear === nowDate.getFullYear() && currentMonth <= nowDate.getMonth() + 1)
          ) {
            monthSet.add(`${currentYear}-${currentMonth}`);
            currentMonth++;
            if (currentMonth > 12) {
              currentMonth = 1;
              currentYear++;
            }
          }
        } else {
          monthSet.add(`${year}-${month}`);
        }
      }
    });

    return Array.from(monthSet)
      .map(key => {
        const [year, month] = key.split('-').map(Number);
        return { year, month };
      })
      .sort((a, b) => {
        if (a.year !== b.year) return b.year - a.year;
        return b.month - a.month;
      });
  }, [incomes, allExpenses]);

  // Рассчитываем баланс по месяцам
  const monthlyBalances = useMemo<MonthlyBalance[]>(() => {
    return monthsWithTransactions.map(({ year, month }) => {
      let monthIncomes = 0;
      let monthExpenses = 0;

      // Считаем доходы за месяц
      incomes.forEach(income => {
        if (isInPeriod(income.operationDate, income.isMonthly, year, month)) {
          monthIncomes += income.amount;
        }
      });

      // Считаем расходы за месяц
      allExpenses.forEach(expense => {
        if (isInPeriod(expense.operationDate, expense.isMonthly, year, month)) {
          monthExpenses += expense.amount;
        }
      });

      return {
        year,
        month,
        incomes: Number(monthIncomes.toFixed(2)),
        expenses: Number(monthExpenses.toFixed(2)),
        balance: Number((monthIncomes - monthExpenses).toFixed(2)),
      };
    });
  }, [incomes, allExpenses, monthsWithTransactions]);

  // Общий баланс (сумма всех балансов)
  const totalBalance = useMemo(
    () => monthlyBalances.reduce((sum, mb) => sum + mb.balance, 0),
    [monthlyBalances]
  );

  const shiftMonth = (delta: number) => {
    setSelectedMonth(prev => {
      let nextMonth = prev + delta;
      let nextYear = selectedYear;
      if (nextMonth > 12) {
        nextMonth = 1;
        nextYear += 1;
      } else if (nextMonth < 1) {
        nextMonth = 12;
        nextYear -= 1;
      }
      setSelectedYear(nextYear);
      return nextMonth;
    });
  };

  // Функция для проверки, попадает ли дата в диапазон
  const isDateInRange = (
    transactionDate: string,
    isMonthly: boolean,
    startYear: number,
    startMonth: number,
    endYear: number,
    endMonth: number
  ): boolean => {
    const { year, month } = getYearMonth(transactionDate);
    if (!year || !month) return false;

    // Для помесячных транзакций: проверяем, попадает ли начало периода в диапазон
    if (isMonthly) {
      // Помесячная транзакция учитывается, если её дата <= конца периода
      return year < endYear || (year === endYear && month <= endMonth);
    }

    // Для разовых транзакций: проверяем точное попадание в диапазон
    const startDate = new Date(startYear, startMonth - 1, 1);
    const endDate = new Date(endYear, endMonth, 0); // Последний день месяца
    const txDate = new Date(year, month - 1, Number(transactionDate.split('-')[2]) || 1);

    return txDate >= startDate && txDate <= endDate;
  };

  // Получить все даты в диапазоне
  const getDatesInRange = (startYear: number, startMonth: number, endYear: number, endMonth: number): Date[] => {
    const dates: Date[] = [];
    const start = new Date(startYear, startMonth - 1, 1);
    const end = new Date(endYear, endMonth, 0); // Последний день месяца

    const current = new Date(start);
    while (current <= end) {
      dates.push(new Date(current));
      current.setDate(current.getDate() + 1);
    }

    return dates;
  };

  // Определить тип группировки в зависимости от размера диапазона
  const getGroupingType = (startYear: number, startMonth: number, endYear: number, endMonth: number): 'day' | 'week' | 'month' => {
    const start = new Date(startYear, startMonth - 1, 1);
    const end = new Date(endYear, endMonth, 0);
    const daysDiff = Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24));

    if (daysDiff <= 31) return 'day';
    if (daysDiff <= 90) return 'week';
    return 'month';
  };

  // Фильтруем расходы для гистограммы по выбранному диапазону
  const barChartFilteredExpenses = useMemo(() => {
    return allExpenses.filter(e =>
      isDateInRange(e.operationDate, e.isMonthly, barChartStartYear, barChartStartMonth, barChartEndYear, barChartEndMonth)
    );
  }, [allExpenses, barChartStartYear, barChartStartMonth, barChartEndYear, barChartEndMonth]);

  // Группируем расходы для гистограммы
  const barChartData = useMemo<BarChartData[]>(() => {
    const groupingType = getGroupingType(barChartStartYear, barChartStartMonth, barChartEndYear, barChartEndMonth);
    const dataMap = new Map<string, { amount: number; count: number; label: string }>();

    // Получаем все месяцы в диапазоне
    const monthsInRange: Array<{ year: number; month: number }> = [];
    let currentYear = barChartStartYear;
    let currentMonth = barChartStartMonth;
    
    while (
      currentYear < barChartEndYear ||
      (currentYear === barChartEndYear && currentMonth <= barChartEndMonth)
    ) {
      monthsInRange.push({ year: currentYear, month: currentMonth });
      currentMonth++;
      if (currentMonth > 12) {
        currentMonth = 1;
        currentYear++;
      }
    }

    if (groupingType === 'day') {
      // Группировка по дням
      const dates = getDatesInRange(barChartStartYear, barChartStartMonth, barChartEndYear, barChartEndMonth);

      // Обрабатываем все расходы (и разовые, и помесячные)
      barChartFilteredExpenses.forEach(expense => {
        const { year: expenseYear, month: expenseMonth } = getYearMonth(expense.operationDate);
        const expenseDay = Number(expense.operationDate.split('-')[2]) || 1;
        
        if (!expense.isMonthly) {
          // Разовый расход - показываем только в день его создания
          const key = expense.operationDate;
          const label = `${expenseDay}.${expenseMonth}`;
          const current = dataMap.get(key) || { amount: 0, count: 0, label };
          dataMap.set(key, {
            amount: current.amount + expense.amount,
            count: current.count + 1,
            label,
          });
        } else {
          // Помесячный расход - показываем в день его создания для каждого месяца в диапазоне
          monthsInRange.forEach(({ year, month }) => {
            // Проверяем, что месяц >= месяца создания расхода
            if (year > expenseYear || (year === expenseYear && month >= expenseMonth)) {
              // Проверяем, что день существует в этом месяце (например, 31 число может не быть в феврале)
              const daysInMonth = new Date(year, month, 0).getDate();
              if (expenseDay <= daysInMonth) {
                const key = `${year}-${String(month).padStart(2, '0')}-${String(expenseDay).padStart(2, '0')}`;
                const label = `${expenseDay}.${month}`;
                const current = dataMap.get(key) || { amount: 0, count: 0, label };
                dataMap.set(key, {
                  amount: current.amount + expense.amount,
                  count: current.count + 1,
                  label,
                });
              }
            }
          });
        }
      });
    } else if (groupingType === 'week') {
      // Группировка по неделям
      const dates = getDatesInRange(barChartStartYear, barChartStartMonth, barChartEndYear, barChartEndMonth);
      const weekMap = new Map<string, { dates: Date[]; label: string }>();

      // Создаем карту недель
      dates.forEach(date => {
        const weekStart = new Date(date);
        weekStart.setDate(date.getDate() - date.getDay()); // Начало недели (воскресенье)
        const weekKey = `${weekStart.getFullYear()}-${String(weekStart.getMonth() + 1).padStart(2, '0')}-${String(weekStart.getDate()).padStart(2, '0')}`;
        
        if (!weekMap.has(weekKey)) {
          const weekDates: Date[] = [];
          for (let i = 0; i < 7; i++) {
            const d = new Date(weekStart);
            d.setDate(weekStart.getDate() + i);
            if (d >= new Date(barChartStartYear, barChartStartMonth - 1, 1) && 
                d <= new Date(barChartEndYear, barChartEndMonth, 0)) {
              weekDates.push(d);
            }
          }
          const firstDate = weekDates[0];
          const lastDate = weekDates[weekDates.length - 1];
          const label = `${firstDate.getDate()}.${firstDate.getMonth() + 1} - ${lastDate.getDate()}.${lastDate.getMonth() + 1}`;
          weekMap.set(weekKey, { dates: weekDates, label });
        }
      });

      // Обрабатываем все расходы (и разовые, и помесячные)
      barChartFilteredExpenses.forEach(expense => {
        const { year: expenseYear, month: expenseMonth } = getYearMonth(expense.operationDate);
        const expenseDay = Number(expense.operationDate.split('-')[2]) || 1;
        
        if (!expense.isMonthly) {
          // Разовый расход - показываем только в неделю его создания
          const expenseDate = new Date(expense.operationDate);
          const weekStart = new Date(expenseDate);
          weekStart.setDate(expenseDate.getDate() - expenseDate.getDay());
          const weekKey = `${weekStart.getFullYear()}-${String(weekStart.getMonth() + 1).padStart(2, '0')}-${String(weekStart.getDate()).padStart(2, '0')}`;
          const week = weekMap.get(weekKey);
          if (week) {
            const current = dataMap.get(weekKey) || { amount: 0, count: 0, label: week.label };
            dataMap.set(weekKey, {
              amount: current.amount + expense.amount,
              count: current.count + 1,
              label: week.label,
            });
          }
        } else {
          // Помесячный расход - показываем в неделю, которая содержит день создания расхода для каждого месяца
          monthsInRange.forEach(({ year, month }) => {
            if (year > expenseYear || (year === expenseYear && month >= expenseMonth)) {
              // Проверяем, что день существует в этом месяце
              const daysInMonth = new Date(year, month, 0).getDate();
              if (expenseDay <= daysInMonth) {
                const expenseDateInMonth = new Date(year, month - 1, expenseDay);
                const weekStart = new Date(expenseDateInMonth);
                weekStart.setDate(expenseDateInMonth.getDate() - expenseDateInMonth.getDay());
                const weekKey = `${weekStart.getFullYear()}-${String(weekStart.getMonth() + 1).padStart(2, '0')}-${String(weekStart.getDate()).padStart(2, '0')}`;
                const week = weekMap.get(weekKey);
                if (week) {
                  const current = dataMap.get(weekKey) || { amount: 0, count: 0, label: week.label };
                  dataMap.set(weekKey, {
                    amount: current.amount + expense.amount,
                    count: current.count + 1,
                    label: week.label,
                  });
                }
              }
            }
          });
        }
      });
    } else {
      // Группировка по месяцам
      monthsInRange.forEach(({ year, month }) => {
        const key = `${year}-${String(month).padStart(2, '0')}`;
        const label = `${MONTH_OPTIONS[month - 1]?.label || month} ${year}`;
        dataMap.set(key, { amount: 0, count: 0, label });
      });

      // Обрабатываем разовые расходы
      barChartFilteredExpenses.forEach(expense => {
        if (!expense.isMonthly) {
          const { year, month } = getYearMonth(expense.operationDate);
          const key = `${year}-${String(month).padStart(2, '0')}`;
          const current = dataMap.get(key);
          if (current) {
            dataMap.set(key, {
              amount: current.amount + expense.amount,
              count: current.count + 1,
              label: current.label,
            });
          }
        }
      });

      // Обрабатываем помесячные расходы
      barChartFilteredExpenses.forEach(expense => {
        if (expense.isMonthly) {
          const { year: expenseYear, month: expenseMonth } = getYearMonth(expense.operationDate);
          
          // Добавляем расход в каждый месяц, начиная с месяца создания
          monthsInRange.forEach(({ year, month }) => {
            if (year > expenseYear || (year === expenseYear && month >= expenseMonth)) {
              const key = `${year}-${String(month).padStart(2, '0')}`;
              const current = dataMap.get(key);
              if (current) {
                dataMap.set(key, {
                  amount: current.amount + expense.amount,
                  count: current.count + 1,
                  label: current.label,
                });
              }
            }
          });
        }
      });
    }

    return Array.from(dataMap.entries())
      .map(([key, { amount, count, label }]) => ({
        date: key,
        amount: Number(amount.toFixed(2)),
        count,
        label,
      }))
      .sort((a, b) => a.date.localeCompare(b.date));
  }, [barChartFilteredExpenses, barChartStartYear, barChartStartMonth, barChartEndYear, barChartEndMonth]);

  const barChartTotalAmount = useMemo(
    () => barChartData.reduce((sum, item) => sum + item.amount, 0),
    [barChartData]
  );

  const availableYearsForBarChart = useMemo(() => {
    const set = new Set<number>([now.getFullYear(), barChartStartYear, barChartEndYear]);
    incomes.forEach(i => {
      const { year } = getYearMonth(i.operationDate);
      if (year) set.add(year);
    });
    allExpenses.forEach(e => {
      const { year } = getYearMonth(e.operationDate);
      if (year) set.add(year);
    });
    return Array.from(set).sort((a, b) => b - a);
  }, [incomes, allExpenses, now, barChartStartYear, barChartEndYear]);

  // Фильтруем доходы для гистограммы по выбранному диапазону
  const incomeChartFilteredIncomes = useMemo(() => {
    return incomes.filter(i =>
      isDateInRange(i.operationDate, i.isMonthly, incomeChartStartYear, incomeChartStartMonth, incomeChartEndYear, incomeChartEndMonth)
    );
  }, [incomes, incomeChartStartYear, incomeChartStartMonth, incomeChartEndYear, incomeChartEndMonth]);

  // Группируем доходы для гистограммы (аналогично расходам)
  const incomeChartData = useMemo<BarChartData[]>(() => {
    const groupingType = getGroupingType(incomeChartStartYear, incomeChartStartMonth, incomeChartEndYear, incomeChartEndMonth);
    const dataMap = new Map<string, { amount: number; count: number; label: string }>();

    // Получаем все месяцы в диапазоне
    const monthsInRange: Array<{ year: number; month: number }> = [];
    let currentYear = incomeChartStartYear;
    let currentMonth = incomeChartStartMonth;
    
    while (
      currentYear < incomeChartEndYear ||
      (currentYear === incomeChartEndYear && currentMonth <= incomeChartEndMonth)
    ) {
      monthsInRange.push({ year: currentYear, month: currentMonth });
      currentMonth++;
      if (currentMonth > 12) {
        currentMonth = 1;
        currentYear++;
      }
    }

    if (groupingType === 'day') {
      // Группировка по дням
      const dates = getDatesInRange(incomeChartStartYear, incomeChartStartMonth, incomeChartEndYear, incomeChartEndMonth);

      // Обрабатываем все доходы (и разовые, и помесячные)
      incomeChartFilteredIncomes.forEach(income => {
        const { year: incomeYear, month: incomeMonth } = getYearMonth(income.operationDate);
        const incomeDay = Number(income.operationDate.split('-')[2]) || 1;
        
        if (!income.isMonthly) {
          // Разовый доход - показываем только в день его создания
          const key = income.operationDate;
          const label = `${incomeDay}.${incomeMonth}`;
          const current = dataMap.get(key) || { amount: 0, count: 0, label };
          dataMap.set(key, {
            amount: current.amount + income.amount,
            count: current.count + 1,
            label,
          });
        } else {
          // Помесячный доход - показываем в день его создания для каждого месяца в диапазоне
          monthsInRange.forEach(({ year, month }) => {
            if (year > incomeYear || (year === incomeYear && month >= incomeMonth)) {
              const daysInMonth = new Date(year, month, 0).getDate();
              if (incomeDay <= daysInMonth) {
                const key = `${year}-${String(month).padStart(2, '0')}-${String(incomeDay).padStart(2, '0')}`;
                const label = `${incomeDay}.${month}`;
                const current = dataMap.get(key) || { amount: 0, count: 0, label };
                dataMap.set(key, {
                  amount: current.amount + income.amount,
                  count: current.count + 1,
                  label,
                });
              }
            }
          });
        }
      });
    } else if (groupingType === 'week') {
      // Группировка по неделям
      const dates = getDatesInRange(incomeChartStartYear, incomeChartStartMonth, incomeChartEndYear, incomeChartEndMonth);
      const weekMap = new Map<string, { dates: Date[]; label: string }>();

      dates.forEach(date => {
        const weekStart = new Date(date);
        weekStart.setDate(date.getDate() - date.getDay());
        const weekKey = `${weekStart.getFullYear()}-${String(weekStart.getMonth() + 1).padStart(2, '0')}-${String(weekStart.getDate()).padStart(2, '0')}`;
        
        if (!weekMap.has(weekKey)) {
          const weekDates: Date[] = [];
          for (let i = 0; i < 7; i++) {
            const d = new Date(weekStart);
            d.setDate(weekStart.getDate() + i);
            if (d >= new Date(incomeChartStartYear, incomeChartStartMonth - 1, 1) && 
                d <= new Date(incomeChartEndYear, incomeChartEndMonth, 0)) {
              weekDates.push(d);
            }
          }
          const firstDate = weekDates[0];
          const lastDate = weekDates[weekDates.length - 1];
          const label = `${firstDate.getDate()}.${firstDate.getMonth() + 1} - ${lastDate.getDate()}.${lastDate.getMonth() + 1}`;
          weekMap.set(weekKey, { dates: weekDates, label });
        }
      });

      incomeChartFilteredIncomes.forEach(income => {
        const { year: incomeYear, month: incomeMonth } = getYearMonth(income.operationDate);
        const incomeDay = Number(income.operationDate.split('-')[2]) || 1;
        
        if (!income.isMonthly) {
          const incomeDate = new Date(income.operationDate);
          const weekStart = new Date(incomeDate);
          weekStart.setDate(incomeDate.getDate() - incomeDate.getDay());
          const weekKey = `${weekStart.getFullYear()}-${String(weekStart.getMonth() + 1).padStart(2, '0')}-${String(weekStart.getDate()).padStart(2, '0')}`;
          const week = weekMap.get(weekKey);
          if (week) {
            const current = dataMap.get(weekKey) || { amount: 0, count: 0, label: week.label };
            dataMap.set(weekKey, {
              amount: current.amount + income.amount,
              count: current.count + 1,
              label: week.label,
            });
          }
        } else {
          monthsInRange.forEach(({ year, month }) => {
            if (year > incomeYear || (year === incomeYear && month >= incomeMonth)) {
              const daysInMonth = new Date(year, month, 0).getDate();
              if (incomeDay <= daysInMonth) {
                const incomeDateInMonth = new Date(year, month - 1, incomeDay);
                const weekStart = new Date(incomeDateInMonth);
                weekStart.setDate(incomeDateInMonth.getDate() - incomeDateInMonth.getDay());
                const weekKey = `${weekStart.getFullYear()}-${String(weekStart.getMonth() + 1).padStart(2, '0')}-${String(weekStart.getDate()).padStart(2, '0')}`;
                const week = weekMap.get(weekKey);
                if (week) {
                  const current = dataMap.get(weekKey) || { amount: 0, count: 0, label: week.label };
                  dataMap.set(weekKey, {
                    amount: current.amount + income.amount,
                    count: current.count + 1,
                    label: week.label,
                  });
                }
              }
            }
          });
        }
      });
    } else {
      // Группировка по месяцам
      monthsInRange.forEach(({ year, month }) => {
        const key = `${year}-${String(month).padStart(2, '0')}`;
        const label = `${MONTH_OPTIONS[month - 1]?.label || month} ${year}`;
        dataMap.set(key, { amount: 0, count: 0, label });
      });

      incomeChartFilteredIncomes.forEach(income => {
        if (!income.isMonthly) {
          const { year, month } = getYearMonth(income.operationDate);
          const key = `${year}-${String(month).padStart(2, '0')}`;
          const current = dataMap.get(key);
          if (current) {
            dataMap.set(key, {
              amount: current.amount + income.amount,
              count: current.count + 1,
              label: current.label,
            });
          }
        } else {
          const { year: incomeYear, month: incomeMonth } = getYearMonth(income.operationDate);
          monthsInRange.forEach(({ year, month }) => {
            if (year > incomeYear || (year === incomeYear && month >= incomeMonth)) {
              const key = `${year}-${String(month).padStart(2, '0')}`;
              const current = dataMap.get(key);
              if (current) {
                dataMap.set(key, {
                  amount: current.amount + income.amount,
                  count: current.count + 1,
                  label: current.label,
                });
              }
            }
          });
        }
      });
    }

    return Array.from(dataMap.entries())
      .map(([key, { amount, count, label }]) => ({
        date: key,
        amount: Number(amount.toFixed(2)),
        count,
        label,
      }))
      .sort((a, b) => a.date.localeCompare(b.date));
  }, [incomeChartFilteredIncomes, incomeChartStartYear, incomeChartStartMonth, incomeChartEndYear, incomeChartEndMonth]);

  const incomeChartTotalAmount = useMemo(
    () => incomeChartData.reduce((sum, item) => sum + item.amount, 0),
    [incomeChartData]
  );

  const availableYearsForIncomeChart = useMemo(() => {
    const set = new Set<number>([now.getFullYear(), incomeChartStartYear, incomeChartEndYear]);
    incomes.forEach(i => {
      const { year } = getYearMonth(i.operationDate);
      if (year) set.add(year);
    });
    return Array.from(set).sort((a, b) => b - a);
  }, [incomes, now, incomeChartStartYear, incomeChartEndYear]);

  // Валидация диапазона дат для доходов
  const isIncomeDateRangeValid = useMemo(() => {
    const start = new Date(incomeChartStartYear, incomeChartStartMonth - 1, 1);
    const end = new Date(incomeChartEndYear, incomeChartEndMonth, 0);
    return start <= end;
  }, [incomeChartStartYear, incomeChartStartMonth, incomeChartEndYear, incomeChartEndMonth]);

  const BarChartTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0];
      return (
        <Paper sx={{ p: 1.5, boxShadow: 3 }}>
          <Typography variant="body2" fontWeight={600}>
            {data.payload.label || data.payload.date}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Сумма: {formatMoney(data.value)}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            Количество транзакций: {data.payload.count}
          </Typography>
        </Paper>
      );
    }
    return null;
  };

  // Валидация диапазона дат
  const isDateRangeValid = useMemo(() => {
    const start = new Date(barChartStartYear, barChartStartMonth - 1, 1);
    const end = new Date(barChartEndYear, barChartEndMonth, 0);
    return start <= end;
  }, [barChartStartYear, barChartStartMonth, barChartEndYear, barChartEndMonth]);

  const CustomTooltip = ({ active, payload }: any) => {
    if (active && payload && payload.length) {
      const data = payload[0];
      return (
        <Paper sx={{ p: 1.5, boxShadow: 3 }}>
          <Typography variant="body2" fontWeight={600}>
            {data.name}
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Сумма: {formatMoney(data.value)}
          </Typography>
          <Typography variant="caption" color="text.secondary">
            Количество: {data.payload.count};
          </Typography>
          <Typography variant="caption" color="text.secondary">
            Доля: {((data.value / totalAmount) * 100).toFixed(1)}%
          </Typography>
        </Paper>
      );
    }
    return null;
  };

  const CustomLabel = ({ cx, cy, midAngle, innerRadius, outerRadius, percent, name }: any) => {
    const RADIAN = Math.PI / 180;
    const radius = innerRadius + (outerRadius - innerRadius) * 0.5;
    const x = cx + radius * Math.cos(-midAngle * RADIAN);
    const y = cy + radius * Math.sin(-midAngle * RADIAN);

    // Показываем метку только если доля больше 5%
    if (percent < 0.05) return null;

    return (
      <text
        x={x}
        y={y}
        fill="white"
        textAnchor={x > cx ? 'start' : 'end'}
        dominantBaseline="central"
        fontSize={12}
        fontWeight={600}
      >
        {name.length > 15 ? `${name.substring(0, 15)}...` : name}
      </text>
    );
  };

  return (
    <>
      <meta name="title" content="Аналитика" />
      <Box sx={{ maxWidth: 1200, mx: 'auto', px: { xs: 2, sm: 3 }, py: 3, display: 'flex', flexDirection: 'column', gap: 3 }}>
        <Stack direction="row" justifyContent="center" alignItems="center">
          <Typography variant="h4">Аналитика расходов</Typography>
        </Stack>

        {/* Баланс по месяцам */}
        {!loadingIncomes && !loadingExpenses && monthlyBalances.length > 0 && (
          <Paper variant="outlined" sx={{ p: 2 }}>
            <Stack spacing={2}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="h6">Баланс по месяцам</Typography>
                <Chip
                  label={`Общий баланс: ${formatMoney(totalBalance)}`}
                  color={totalBalance >= 0 ? 'success' : 'error'}
                  sx={{ fontWeight: 600 }}
                />
              </Stack>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      <TableCell>Месяц</TableCell>
                      <TableCell align="right">Доходы</TableCell>
                      <TableCell align="right">Расходы</TableCell>
                      <TableCell align="right">Баланс</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {monthlyBalances.map((mb) => (
                      <TableRow key={`${mb.year}-${mb.month}`}>
                        <TableCell>
                          {MONTH_OPTIONS[mb.month - 1]?.label || mb.month} {mb.year}
                        </TableCell>
                        <TableCell align="right" sx={{ color: 'success.main' }}>
                          +{formatMoney(mb.incomes)}
                        </TableCell>
                        <TableCell align="right" sx={{ color: 'error.main' }}>
                          -{formatMoney(mb.expenses)}
                        </TableCell>
                        <TableCell
                          align="right"
                          sx={{
                            fontWeight: 600,
                            color: mb.balance >= 0 ? 'success.main' : 'error.main',
                          }}
                        >
                          {mb.balance >= 0 ? '+' : ''}
                          {formatMoney(mb.balance)}
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Stack>
          </Paper>
        )}

        {(loadingIncomes || loadingExpenses) ? (
          <Stack alignItems="center" justifyContent="center" sx={{ py: 5 }}>
            <CircularProgress size={40} />
            <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
              {loadingIncomes ? 'Загрузка доходов...' : 'Загрузка расходов...'}
            </Typography>
          </Stack>
        ) : error ? (
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="body1" color="error" align="center">
              {error}
            </Typography>
          </Paper>
        ) : expenseGroups.length === 0 ? (
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Typography variant="body1" color="text.secondary" align="center">
              Нет расходов за выбранный период
            </Typography>
          </Paper>
        ) : (
          <Paper variant="outlined" sx={{ p: 3 }}>
            <Stack spacing={3}>
              <Stack direction="row" justifyContent="space-between" alignItems="center">
                <Typography variant="h6">Распределение расходов</Typography>
                <Typography variant="body1" fontWeight={600}>
                  Всего: {formatMoney(totalAmount)}
                </Typography>
              </Stack>

              <Stack direction="row" spacing={1} alignItems="center" justifyContent="center" flexWrap="wrap" rowGap={1.5}>
              <Button
                variant="outlined"
                size="small"
                onClick={() => shiftMonth(-1)}
                aria-label="Предыдущий месяц"
              >
                <ArrowBackIosNewIcon fontSize="small" />
              </Button>
              <TextField
                select
                size="small"
                label="Месяц"
                value={selectedMonth}
                onChange={e => setSelectedMonth(Number(e.target.value))}
                sx={{ minWidth: { xs: 140, sm: 180 } }}
              >
                {MONTH_OPTIONS.map(m => (
                  <MenuItem key={m.value} value={m.value}>
                    {m.label}
                  </MenuItem>
                ))}
              </TextField>
              <TextField
                select
                size="small"
                label="Год"
                value={selectedYear}
                onChange={e => setSelectedYear(Number(e.target.value))}
                sx={{ minWidth: { xs: 110, sm: 140 } }}
              >
                {availableYears.map(y => (
                  <MenuItem key={y} value={y}>
                    {y}
                  </MenuItem>
                ))}
              </TextField>
              <Button
                variant="outlined"
                size="small"
                onClick={() => shiftMonth(1)}
                aria-label="Следующий месяц"
              >
                <ArrowForwardIosIcon fontSize="small" />
              </Button>
            </Stack>

              <Box sx={{ width: '100%', height: 500 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <PieChart>
                    <Pie
                      data={expenseGroups}
                      cx="50%"
                      cy="50%"
                      labelLine={false}
                      label={CustomLabel}
                      outerRadius={180}
                      fill="#8884d8"
                      dataKey="value"
                    >
                      {expenseGroups.map((_, index) => (
                        <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Pie>
                    <Tooltip content={<CustomTooltip />} />
                    <Legend
                      formatter={(value) => {
                        const group = expenseGroups.find(g => g.name === value);
                        if (!group) return value;
                        const percent = ((group.value / totalAmount) * 100).toFixed(1);
                        return `${value} (${percent}%)`;
                      }}
                    />
                  </PieChart>
                </ResponsiveContainer>
              </Box>

              <Box>
                <Typography variant="subtitle2" gutterBottom>
                  Детализация по расходам:
                </Typography>
                <Stack spacing={1} sx={{ mt: 1 }}>
                  {expenseGroups.map((group, index) => {
                    const percent = ((group.value / totalAmount) * 100).toFixed(1);
                    return (
                      <Stack
                        key={group.name}
                        direction="row"
                        justifyContent="space-between"
                        alignItems="center"
                        sx={{
                          p: 1.5,
                          borderRadius: 1,
                          bgcolor: 'action.hover',
                        }}
                      >
                        <Stack direction="row" spacing={1} alignItems="center">
                          <Box
                            sx={{
                              width: 16,
                              height: 16,
                              borderRadius: '50%',
                              bgcolor: COLORS[index % COLORS.length],
                            }}
                          />
                          <Typography variant="body2">{group.name}</Typography>
                        </Stack>
                        <Stack direction="row" spacing={2} alignItems="center">
                          <Typography variant="body2" color="text.secondary">
                            {group.count} {group.count === 1 ? 'транзакция' : group.count < 5 ? 'транзакции' : 'транзакций'}
                          </Typography>
                          <Typography variant="body2" fontWeight={600}>
                            {formatMoney(group.value)}
                          </Typography>
                          <Typography variant="body2" color="text.secondary">
                            ({percent}%)
                          </Typography>
                        </Stack>
                      </Stack>
                    );
                  })}
                </Stack>
              </Box>
            </Stack>
          </Paper>
        )}

        {/* Гистограмма доходов */}
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Stack spacing={2}>
            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <Typography variant="subtitle1" fontWeight={600}>
                Доходы за период
              </Typography>
              <Typography variant="body2" fontWeight={600}>
                Всего: {formatMoney(incomeChartTotalAmount)}
              </Typography>
            </Stack>

            {!isIncomeDateRangeValid && (
              <Typography variant="body2" color="error" align="center">
                Дата начала должна быть раньше или равна дате конца
              </Typography>
            )}

            <Stack spacing={2}>
              <Stack direction="row" spacing={1} alignItems="center" justifyContent="center" flexWrap="wrap" rowGap={1.5}>
                <Typography variant="body2" fontWeight={600} sx={{ minWidth: 80 }}>
                  Начало:
                </Typography>
                <TextField
                  select
                  size="small"
                  label="Месяц"
                  value={incomeChartStartMonth}
                  onChange={e => setIncomeChartStartMonth(Number(e.target.value))}
                  sx={{ minWidth: { xs: 140, sm: 180 } }}
                >
                  {MONTH_OPTIONS.map(m => (
                    <MenuItem key={m.value} value={m.value}>
                      {m.label}
                    </MenuItem>
                  ))}
                </TextField>
                <TextField
                  select
                  size="small"
                  label="Год"
                  value={incomeChartStartYear}
                  onChange={e => setIncomeChartStartYear(Number(e.target.value))}
                  sx={{ minWidth: { xs: 110, sm: 140 } }}
                >
                  {availableYearsForIncomeChart.map(y => (
                    <MenuItem key={y} value={y}>
                      {y}
                    </MenuItem>
                  ))}
                </TextField>
              </Stack>

              <Stack direction="row" spacing={1} alignItems="center" justifyContent="center" flexWrap="wrap" rowGap={1.5}>
                <Typography variant="body2" fontWeight={600} sx={{ minWidth: 80 }}>
                  Конец:
                </Typography>
                <TextField
                  select
                  size="small"
                  label="Месяц"
                  value={incomeChartEndMonth}
                  onChange={e => setIncomeChartEndMonth(Number(e.target.value))}
                  sx={{ minWidth: { xs: 140, sm: 180 } }}
                >
                  {MONTH_OPTIONS.map(m => (
                    <MenuItem key={m.value} value={m.value}>
                      {m.label}
                    </MenuItem>
                  ))}
                </TextField>
                <TextField
                  select
                  size="small"
                  label="Год"
                  value={incomeChartEndYear}
                  onChange={e => setIncomeChartEndYear(Number(e.target.value))}
                  sx={{ minWidth: { xs: 110, sm: 140 } }}
                >
                  {availableYearsForIncomeChart.map(y => (
                    <MenuItem key={y} value={y}>
                      {y}
                    </MenuItem>
                  ))}
                </TextField>
              </Stack>
            </Stack>

            {!isIncomeDateRangeValid ? (
              <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 3 }}>
                Выберите корректный диапазон дат
              </Typography>
            ) : incomeChartData.length === 0 || incomeChartTotalAmount === 0 ? (
              <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 3 }}>
                Нет доходов за выбранный период
              </Typography>
            ) : (
              <Box sx={{ width: '100%', height: 400 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={incomeChartData} margin={{ top: 20, right: 30, left: 20, bottom: 80 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis
                      dataKey="label"
                      label={{ value: 'Период', position: 'insideBottom', offset: -5 }}
                      angle={-45}
                      textAnchor="end"
                      height={100}
                    />
                    <YAxis
                      label={{ value: 'Сумма (₽)', angle: -90, position: 'insideLeft' }}
                      tickFormatter={(value) => value.toFixed(0)}
                    />
                    <Tooltip content={<BarChartTooltip />} />
                    <Bar dataKey="amount" fill="#00C49F" radius={[4, 4, 0, 0]}>
                      {incomeChartData.map((_, index) => (
                        <Cell key={`income-bar-cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </Box>
            )}
          </Stack>
        </Paper>

        {/* Гистограмма расходов */}
        <Paper variant="outlined" sx={{ p: 2 }}>
          <Stack spacing={2}>
            <Stack direction="row" justifyContent="space-between" alignItems="center">
              <Typography variant="subtitle1" fontWeight={600}>
                Расходы за период
              </Typography>
              <Typography variant="body2" fontWeight={600}>
                Всего: {formatMoney(barChartTotalAmount)}
              </Typography>
            </Stack>

            {!isDateRangeValid && (
              <Typography variant="body2" color="error" align="center">
                Дата начала должна быть раньше или равна дате конца
              </Typography>
            )}

            <Stack spacing={2}>
              <Stack direction="row" spacing={1} alignItems="center" justifyContent="center" flexWrap="wrap" rowGap={1.5}>
                <Typography variant="body2" fontWeight={600} sx={{ minWidth: 80 }}>
                  Начало:
                </Typography>
                <TextField
                  select
                  size="small"
                  label="Месяц"
                  value={barChartStartMonth}
                  onChange={e => setBarChartStartMonth(Number(e.target.value))}
                  sx={{ minWidth: { xs: 140, sm: 180 } }}
                >
                  {MONTH_OPTIONS.map(m => (
                    <MenuItem key={m.value} value={m.value}>
                      {m.label}
                    </MenuItem>
                  ))}
                </TextField>
                <TextField
                  select
                  size="small"
                  label="Год"
                  value={barChartStartYear}
                  onChange={e => setBarChartStartYear(Number(e.target.value))}
                  sx={{ minWidth: { xs: 110, sm: 140 } }}
                >
                  {availableYearsForBarChart.map(y => (
                    <MenuItem key={y} value={y}>
                      {y}
                    </MenuItem>
                  ))}
                </TextField>
              </Stack>

              <Stack direction="row" spacing={1} alignItems="center" justifyContent="center" flexWrap="wrap" rowGap={1.5}>
                <Typography variant="body2" fontWeight={600} sx={{ minWidth: 80 }}>
                  Конец:
                </Typography>
                <TextField
                  select
                  size="small"
                  label="Месяц"
                  value={barChartEndMonth}
                  onChange={e => setBarChartEndMonth(Number(e.target.value))}
                  sx={{ minWidth: { xs: 140, sm: 180 } }}
                >
                  {MONTH_OPTIONS.map(m => (
                    <MenuItem key={m.value} value={m.value}>
                      {m.label}
                    </MenuItem>
                  ))}
                </TextField>
                <TextField
                  select
                  size="small"
                  label="Год"
                  value={barChartEndYear}
                  onChange={e => setBarChartEndYear(Number(e.target.value))}
                  sx={{ minWidth: { xs: 110, sm: 140 } }}
                >
                  {availableYearsForBarChart.map(y => (
                    <MenuItem key={y} value={y}>
                      {y}
                    </MenuItem>
                  ))}
                </TextField>
              </Stack>
            </Stack>

            {!isDateRangeValid ? (
              <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 3 }}>
                Выберите корректный диапазон дат
              </Typography>
            ) : barChartData.length === 0 || barChartTotalAmount === 0 ? (
              <Typography variant="body2" color="text.secondary" align="center" sx={{ py: 3 }}>
                Нет расходов за выбранный период
              </Typography>
            ) : (
              <Box sx={{ width: '100%', height: 400 }}>
                <ResponsiveContainer width="100%" height="100%">
                  <BarChart data={barChartData} margin={{ top: 20, right: 30, left: 20, bottom: 80 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis
                      dataKey="label"
                      label={{ value: 'Период', position: 'insideBottom', offset: -5 }}
                      angle={-45}
                      textAnchor="end"
                      height={100}
                    />
                    <YAxis
                      label={{ value: 'Сумма (₽)', angle: -90, position: 'insideLeft' }}
                      tickFormatter={(value) => value.toFixed(0)}
                    />
                    <Tooltip content={<BarChartTooltip />} />
                    <Bar dataKey="amount" fill="#8884d8" radius={[4, 4, 0, 0]}>
                      {barChartData.map((_, index) => (
                        <Cell key={`bar-cell-${index}`} fill={COLORS[index % COLORS.length]} />
                      ))}
                    </Bar>
                  </BarChart>
                </ResponsiveContainer>
              </Box>
            )}
          </Stack>
        </Paper>
      </Box>
    </>
  );
}

export default AnalyticsPage;
