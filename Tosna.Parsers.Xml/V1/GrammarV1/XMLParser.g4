parser grammar XMLParser;

options { tokenVocab=XMLLexer; }

document    :   prolog? misc* element (misc duplicateElement)* misc* EOF ;

prolog      :   XMLDeclOpen attribute* SPECIAL_CLOSE ;

content     :   chardata?
                ((element | reference | CDATA | PI | COMMENT) chardata?)* ;

element     :  validElement
            |  invalidElement
            ;
            
duplicateElement: element;

validElement
            :   validOpen content validClose
            |   validOpenShort
            ;
            
invalidElement // like <SomeUnfinishedElement [without '/>']
            :   validOpen content invalidClose
            |   invalidOpen
            ;
            
validOpen   :  OPEN Name attribute* CLOSE;

validClose  :  OPEN SLASH Name CLOSE;

validOpenShort:
               OPEN Name attribute* SLASH_CLOSE;

invalidOpen :  OPEN Name?;

invalidClose:  (OPEN (SLASH Name?)?)?;

reference   :   EntityRef | CharRef ;

attribute   :   Name '=' STRING ; // Our STRING is AttValue in spec

/** ``All text that is not markup constitutes the character data of
 *  the document.''
 */
chardata    :   TEXT | SEA_WS ;

misc        :   COMMENT | PI | SEA_WS ;