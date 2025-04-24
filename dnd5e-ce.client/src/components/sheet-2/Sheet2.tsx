import Sheet2Header from './Sheet2Header'
import Sheet2Body from './Sheet2Body';

import Form from 'react-bootstrap/Form';
import Container  from 'react-bootstrap/Container';

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