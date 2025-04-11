import Sheet2Header from './Sheet2Header'
import Sheet2Body from './Sheet2Body';

import Form from 'react-bootstrap/Form';
import Container  from 'react-bootstrap/Container';

function Sheet2({
  character,
  setCharacter,
  characterName,
  setCharacterName
}) {

  const onCharacterNameChange = (changes) => {
    setCharacterName(changes.name);
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onHeaderColChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet2: {
        ...prev.sheet2,
        ...changes
      }
    }));
  }

  return (
    <Form>
      <Container>
        <Sheet2Header
          character={character}
          characterName={characterName}
          onCharacterNameChange={onCharacterNameChange}
          onHeaderColChange={onHeaderColChange}
        />
        <Sheet2Body
          character={character}
          onHeaderColChange={onHeaderColChange}
        />
      </Container>
    </Form>
  );
}

export default Sheet2;