import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useAppSelector, useAppDispatch } from '../hooks/index';
import { selectAccessToken } from '../store/selectors/authSelectors';
import { Container, Row, Col, Button, Card } from 'react-bootstrap';
import Sheet1 from './sheet-1/Sheet1';
import Sheet2 from './sheet-2/Sheet2';
import Sheet3 from './sheet-3/Sheet3';

const Dashboard: React.FC = () => {
  const [activeSheet, setActiveSheet] = useState<'sheet1' | 'sheet2' | 'sheet3'>('sheet1');

  return (
    <Container className="mt-3">
      <Row className="mb-5">
        <Col>
          <Button
            variant={activeSheet === 'sheet1' ? 'primary' : 'outline-primary'}
            onClick={() => setActiveSheet('sheet1')}
            className="me-2"
          >
            Основное
          </Button>
          <Button
            variant={activeSheet === 'sheet2' ? 'primary' : 'outline-primary'}
            onClick={() => setActiveSheet('sheet2')}
            className="me-2"
          >
            Описание персонажа
          </Button>
          <Button
            variant={activeSheet === 'sheet3' ? 'primary' : 'outline-primary'}
            onClick={() => setActiveSheet('sheet3')}
          >
            Заклинания
          </Button>
        </Col>
      </Row>
      {activeSheet === 'sheet1' && <Sheet1 />}
      {activeSheet === 'sheet2' && <Sheet2 />}
      {activeSheet === 'sheet3' && <Sheet3 />}
    </Container>
  );
}

export default Dashboard;