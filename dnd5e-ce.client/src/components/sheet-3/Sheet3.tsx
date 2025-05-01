import Sheet3Header from './Sheet3Header'
import Sheet3Body from './Sheet3Body';

import Form from 'react-bootstrap/Form';
import Container  from 'react-bootstrap/Container';

const Sheet3: React.FC = () => {
  return (
    <Form>
      <Container>
        <Sheet3Header />
        <Sheet3Body />
      </Container>
    </Form>
  );
}

export default Sheet3;