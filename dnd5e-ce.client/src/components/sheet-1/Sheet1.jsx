import Sheet1Header from './Sheet1Header'
import Sheet1Body from './Sheet1Body';

import Form from 'react-bootstrap/Form';
import Container  from 'react-bootstrap/Container';

function Sheet1({
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

  const updateSheet1HeaderCol = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
    //console.log(character);
  }

  const onLevelUpdate = (changes) => {
    changes.level = Number(changes.level);
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onExperienceUpdate = (changes) => {
    changes.experience = Number(changes.experience);
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onCharacteristicChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        characteristics: {
          ...prev.sheet1.characteristics,
          ...changes
        }
      }
    }));
  }

  const onInspirationChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onSaveThrowProficiencyChange = (fieldName, changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        saveThrows: {
          ...prev.sheet1.saveThrows,
          [fieldName]: {
            ...prev.sheet1.saveThrows[fieldName],
            ...changes
          }
        }
      }
    }));
  }

  const onSkillProficiencyChange = (fieldName, changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        skills: {
          ...prev.sheet1.skills,
          [fieldName]: {
            ...prev.sheet1.skills[fieldName],
            ...changes
          }
        }
      }
    }));
  }

  const onToolsChange = (changes) => { // tools array expect
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        tools: [
          ...changes
        ]
      }
    }));
  };

  const onToolChange = (toolId, changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        tools: prev.sheet1.tools.map(tool =>
          tool.id === toolId ? { ...tool, ...changes } : tool
        )
      }
    }));
  };

  const onArmorClassChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onCharacterInitiativeChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onCharacterSpeedChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onCharacterMaxHPChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        hp: {
          ...prev.sheet1.hp,
          ...changes
        }
      }
    }));
  }

  const onCharacterCurrentHPChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        hp: {
          ...prev.sheet1.hp,
          ...changes
        }
      }
    }));
  }

  const onCharacterTempHPChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        hp: {
          ...prev.sheet1.hp,
          ...changes
        }
      }
    }));
  }

  const onHitDiceTotalChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        hitDice: {
          ...prev.sheet1.hitDice,
          ...changes
        }
      }
    }));
  }

  const onHitDiceCurrentChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        hitDice: {
          ...prev.sheet1.hitDice,
          ...changes
        }
      }
    }));
  }

  const onHitDiceTypeChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        hitDice: {
          ...prev.sheet1.hitDice,
          ...changes
        }
      }
    }));
  }

  const onDeathSaveThrowSuccessesChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        deathSaveThrow: {
          ...prev.sheet1.deathSaveThrow,
          ...changes
        }
      }
    }));
  }

  const onDeathSaveThrowFailuresChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        deathSaveThrow: {
          ...prev.sheet1.deathSaveThrow,
          ...changes
        }
      }
    }));
  }

  const onCharacterPersonalityTraitsChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onCharacterIdealsChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onCharacterBondsChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onCharacterFlawsChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        ...changes
      }
    }));
  }

  const onCharacterClassResourceTotalChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        classResource: {
          ...prev.sheet1.classResource,
          ...changes
        }
      }
    }));
  }

  const onCharacterClassResourceCurrentChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        classResource: {
          ...prev.sheet1.classResource,
          ...changes
        }
      }
    }));
  }

  const onCharacterClassResourceNameChange = (changes) => {
    setCharacter(prev => ({
      ...prev,
      sheet1: {
        ...prev.sheet1,
        classResource: {
          ...prev.sheet1.classResource,
          ...changes
        }
      }
    }));
  }

  return (
    <Form>
      <Container>
        <Sheet1Header
          character={character}
          characterName={characterName}
          onCharacterNameChange={onCharacterNameChange}
          onHeaderUpdate={updateSheet1HeaderCol}
          onLevelUpdate={onLevelUpdate}
          onExperienceUpdate={onExperienceUpdate}
        />
        <Sheet1Body
          character={character}
          onCharacteristicChange={onCharacteristicChange}
          onInspirationChange={onInspirationChange}
          onSaveThrowProficiencyChange={onSaveThrowProficiencyChange}
          onSkillProficiencyChange={onSkillProficiencyChange}
          onToolsChange={onToolsChange}
          onToolChange={onToolChange}
          onArmorClassChange={onArmorClassChange}
          onCharacterInitiativeChange={onCharacterInitiativeChange}
          onCharacterSpeedChange={onCharacterSpeedChange}
          onCharacterMaxHPChange={onCharacterMaxHPChange}
          onCharacterCurrentHPChange={onCharacterCurrentHPChange}
          onCharacterTempHPChange={onCharacterTempHPChange}
          onHitDiceTotalChange={onHitDiceTotalChange}
          onHitDiceCurrentChange={onHitDiceCurrentChange}
          onHitDiceTypeChange={onHitDiceTypeChange}
          onDeathSaveThrowSuccessesChange={onDeathSaveThrowSuccessesChange}
          onDeathSaveThrowFailuresChange={onDeathSaveThrowFailuresChange}
          onCharacterPersonalityTraitsChange={onCharacterPersonalityTraitsChange}
          onCharacterIdealsChange={onCharacterIdealsChange}
          onCharacterBondsChange={onCharacterBondsChange}
          onCharacterFlawsChange={onCharacterFlawsChange}
          onCharacterClassResourceTotalChange={onCharacterClassResourceTotalChange}
          onCharacterClassResourceCurrentChange={onCharacterClassResourceCurrentChange}
          onCharacterClassResourceNameChange={onCharacterClassResourceNameChange}
        />
      </Container>
    </Form>
  );
}

export default Sheet1;