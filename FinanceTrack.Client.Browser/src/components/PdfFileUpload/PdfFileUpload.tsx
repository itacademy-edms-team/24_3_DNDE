import { useCallback } from 'react';
import { useDropzone } from 'react-dropzone';
import { Box, Button, Typography } from '@mui/material';
import UploadFileIcon from '@mui/icons-material/UploadFile';
import InsertDriveFileIcon from '@mui/icons-material/InsertDriveFile';
import ClearIcon from '@mui/icons-material/Clear';

type PdfFileUploadProps = {
  value: File | null;
  onChange: (file: File | null) => void;
  error?: string;
};

const MAX_SIZE = 10 * 1024 * 1024;

function PdfFileUpload({ value, onChange, error }: PdfFileUploadProps) {
  const onDrop = useCallback(
    (acceptedFiles: File[]) => {
      if (acceptedFiles.length > 0) {
        onChange(acceptedFiles[0]);
      }
    },
    [onChange],
  );

  const { getRootProps, getInputProps, isDragActive, fileRejections } = useDropzone({
    onDrop,
    accept: { 'application/pdf': ['.pdf'] },
    maxSize: MAX_SIZE,
    maxFiles: 1,
  });

  const rejectionMessage = fileRejections[0]?.errors[0]?.message;
  const displayError = error || rejectionMessage;

  const handleClear = (e: React.MouseEvent) => {
    e.stopPropagation();
    onChange(null);
  };

  return (
    <Box>
      <Box
        {...getRootProps()}
        sx={{
          border: '2px dashed',
          borderColor: isDragActive ? 'primary.main' : displayError ? 'error.main' : 'divider',
          borderRadius: 2,
          p: 3,
          textAlign: 'center',
          cursor: 'pointer',
          bgcolor: isDragActive ? 'action.hover' : 'background.paper',
          transition: 'border-color 0.2s, background-color 0.2s',
          '&:hover': {
            borderColor: 'primary.main',
            bgcolor: 'action.hover',
          },
        }}
      >
        <input {...getInputProps()} />

        {value ? (
          <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: 1 }}>
            <InsertDriveFileIcon color="primary" />
            <Typography variant="body2" sx={{ flexGrow: 1, textAlign: 'left' }}>
              {value.name}
            </Typography>
            <Button
              size="small"
              color="inherit"
              onClick={handleClear}
              startIcon={<ClearIcon />}
              sx={{ flexShrink: 0 }}
            >
              Убрать
            </Button>
          </Box>
        ) : (
          <>
            <UploadFileIcon sx={{ fontSize: 40, color: 'text.secondary', mb: 1 }} />
            <Typography variant="body2" color="text.secondary">
              {isDragActive ? 'Отпустите файл здесь' : 'Перетащите PDF-файл сюда или'}
            </Typography>
            {!isDragActive && (
              <Button size="small" sx={{ mt: 1 }}>
                Выбрать файл
              </Button>
            )}
            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
              Только PDF, максимум 10 МБ
            </Typography>
          </>
        )}
      </Box>

      {displayError && (
        <Typography variant="caption" color="error" sx={{ mt: 0.5, display: 'block' }}>
          {displayError}
        </Typography>
      )}
    </Box>
  );
}

export default PdfFileUpload;
