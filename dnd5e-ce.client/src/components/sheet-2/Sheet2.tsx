import Container from 'react-bootstrap/Container';
import Form from 'react-bootstrap/Form';

import Sheet2Body from './Sheet2Body';
import Sheet2Header from './Sheet2Header';

const Sheet2: React.FC = () => {
  return (
    <Form>
      <Container>
        <Sheet2Header />
        <Sheet2Body />
      </Container>
    </Form>
  );
}

export default Sheet2;