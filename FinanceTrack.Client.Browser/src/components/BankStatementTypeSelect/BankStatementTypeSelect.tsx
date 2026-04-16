import { Link } from 'react-router';
import {
  Box,
  FormControl,
  FormHelperText,
  InputLabel,
  Link as MuiLink,
  ListSubheader,
  MenuItem,
  Select,
  Typography,
} from '@mui/material';

export type BankStatementType = 'SberbankPayment000001' | 'GazprombankDebit000001';

type Extractor = {
  value: BankStatementType;
  docsPath: string;
};

type StatementCategory = {
  name: string;
  extractors: Extractor[];
};

type Bank = {
  name: string;
  categories: StatementCategory[];
};

// Структура зеркалит бэкенд: ./BankName/CategoryName/ExtractorName000001
const BANKS: Bank[] = [
  {
    name: 'Сбербанк',
    categories: [
      {
        name: 'Выписка по платёжному счёту',
        extractors: [
          { value: 'SberbankPayment000001', docsPath: '/docs/sberbank/payment' },
        ],
      },
    ],
  },
  {
    name: 'Газпромбанк',
    categories: [
      {
        name: 'Выписка по счёту дебетовой карты',
        extractors: [
          { value: 'GazprombankDebit000001', docsPath: '/docs/gazprombank/debit' },
        ],
      },
    ],
  },
];

const ALL_EXTRACTORS: Extractor[] = BANKS.flatMap((b) =>
  b.categories.flatMap((c) => c.extractors),
);

type BankStatementTypeSelectProps = {
  value: BankStatementType | '';
  onChange: (value: BankStatementType) => void;
  error?: string;
};

function BankStatementTypeSelect({ value, onChange, error }: BankStatementTypeSelectProps) {
  const items = BANKS.flatMap((bank) => [
    <ListSubheader key={bank.name}>{bank.name}</ListSubheader>,
    ...bank.categories.flatMap((category) => [
      <ListSubheader
        key={`${bank.name}/${category.name}`}
        sx={{ pl: 4, fontSize: '0.75rem', lineHeight: '28px', color: 'text.secondary' }}
      >
        {category.name}
      </ListSubheader>,
      ...category.extractors.map((extractor) => (
        <MenuItem key={extractor.value} value={extractor.value} sx={{ pl: 6 }}>
          <Box>
            <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
              {extractor.value}
            </Typography>
            <MuiLink
              component={Link}
              to={extractor.docsPath}
              target="_blank"
              rel="noopener"
              variant="caption"
              onClick={(e: React.MouseEvent) => e.stopPropagation()}
            >
              Смотреть образец PDF
            </MuiLink>
          </Box>
        </MenuItem>
      )),
    ]),
  ]);

  return (
    <FormControl fullWidth required error={!!error}>
      <InputLabel>Тип банковской выписки</InputLabel>
      <Select
        value={value}
        label="Тип банковской выписки"
        onChange={(e) => onChange(e.target.value as BankStatementType)}
        renderValue={(selected) => {
          const extractor = ALL_EXTRACTORS.find((e) => e.value === selected);
          return extractor ? (
            <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
              {selected}
            </Typography>
          ) : '';
        }}
      >
        {items}
      </Select>
      {error && <FormHelperText>{error}</FormHelperText>}
    </FormControl>
  );
}

export default BankStatementTypeSelect;
